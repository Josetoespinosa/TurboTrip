using System.Collections;
using UnityEngine;

// Simple enemy that detects the player within a radius, chases and "catches" them (calls respawn)
public class Homeless_script : MonoBehaviour
{
    [Header("Detection")]
    public float detectionRadius = 6f;
    [Tooltip("Distance at which the enemy will attempt to catch/attack (stop distance)")]
    public float catchDistance = 0.6f;
    [Tooltip("If player leaves this distance ( > detectionRadius * loseFactor ) the enemy gives up chase")]
    public float loseFactor = 1f;

    [Header("Movement")]
    public float chaseSpeed = 2f;
    public float patrolSpeed = 1.5f;
    public bool patrolWhenIdle = true;
    public float patrolRange = 5f; // horizontal range from spawn when patrolling
    [Tooltip("Layers considered obstacles for the enemy (walls, ground, platforms)")]
    public LayerMask obstacleMask = ~0;
    [Tooltip("Radius used for obstacle checks (circlecast)")]
    public float obstacleCheckRadius = 0.2f;

    // references
    private Transform player;
    private Rigidbody2D rb;
    private Vector3 spawnPosition;
    // simple state machine for clearer AI behaviour
    private enum State { Idle, Patrol, Chase, ReturnToSpawn, Attack }
    private State state = State.Idle;
    [Header("Animation")]
    public Animator animator;
    public string runningParam = "Running";
    [Tooltip("Trigger name for the attack animation")]
    public string attackTrigger = "Attack";
    [Tooltip("Optional: name of the attack clip inside the animator controller to auto-measure delay")]
    public string attackClipName = "";
    [Tooltip("Fallback delay used if attack clip not found (seconds)")]
    public float attackFallbackDelay = 0.35f;
    // prevent overlapping attack coroutines
    private bool isAttacking = false;
    [Header("AI Settings")]
    [Tooltip("Require an unobstructed line of sight (raycast) to begin/maintain chase")]
    public bool requireLineOfSight = true;
    [Tooltip("Layers considered blocking line of sight (walls, platforms)")]
    public LayerMask lineOfSightMask;
    [Tooltip("Acceleration used to smooth horizontal velocity when using Rigidbody2D")]
    public float accel = 30f;
    [Tooltip("Distance to check for ground ahead to avoid walking off edges")]
    public float groundCheckDistance = 0.6f;
    [Tooltip("Layer mask used for ground detection (default to obstacleMask if left empty)")]
    public LayerMask groundMask;
    [Tooltip("Return-to-spawn tolerance (units)")]
    public float returnTolerance = 0.15f;

    // respawn manager reference to call KillPlayer
    private PlayerRespawnManager respawnManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
        EnsureRespawnRef();
        // initial state
        state = patrolWhenIdle ? State.Patrol : State.Idle;
        // default masks
        if (lineOfSightMask == 0) lineOfSightMask = obstacleMask;
        if (groundMask == 0) groundMask = obstacleMask;
    }

    void EnsureRespawnRef()
    {
#if UNITY_2023_1_OR_NEWER
        if (!respawnManager) respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
#else
        if (!respawnManager) respawnManager = FindObjectOfType<PlayerRespawnManager>();
#endif
    }

    void Update()
    {
        // Lazy-find player by tag
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:
                if (dist <= detectionRadius && (!requireLineOfSight || HasLineOfSight()))
                {
                    state = State.Chase;
                }
                else
                {
                    if (animator != null) animator.SetBool(runningParam, false);
                }
                break;

            case State.Patrol:
                // patrol behaviour (may flip internally)
                PatrolBehaviour();
                if (dist <= detectionRadius && (!requireLineOfSight || HasLineOfSight()))
                {
                    state = State.Chase;
                }
                break;

            case State.Chase:
                // if player too far, transition to return state
                if (dist > detectionRadius * loseFactor)
                {
                    state = State.ReturnToSpawn;
                    break;
                }
                // if LOS required and lost, give up chase
                if (requireLineOfSight && !HasLineOfSight())
                {
                    state = State.ReturnToSpawn;
                    break;
                }

                // move towards player horizontally but avoid edges/obstacles
                Vector2 dir = (player.position - transform.position);
                float dirSign = Mathf.Sign(dir.x);
                // if there is no ground ahead in the chase direction, stop rather than fall
                if (!IsGroundAhead(dirSign))
                {
                    Move(Vector2.zero);
                }
                else
                {
                    Move(new Vector2(dirSign * chaseSpeed, 0f));
                    FlipTowards(player.position.x);
                }

                // if close enough, perform attack (then kill)
                if (dist <= catchDistance)
                {
                    if (!isAttacking)
                    {
                        state = State.Attack;
                        StartCoroutine(AttackThenKill());
                    }
                }
                break;

            case State.ReturnToSpawn:
                // move back to spawn and resume patrol or idle
                Vector2 toSpawn = (spawnPosition - transform.position);
                if (toSpawn.magnitude <= returnTolerance)
                {
                    state = patrolWhenIdle ? State.Patrol : State.Idle;
                    break;
                }
                float rsSign = Mathf.Sign(toSpawn.x);
                // avoid walking off edges when returning
                if (!IsGroundAhead(rsSign))
                {
                    Move(Vector2.zero);
                }
                else
                {
                    Move(new Vector2(rsSign * chaseSpeed, 0f));
                    FlipTowards(spawnPosition.x);
                }
                break;

            case State.Attack:
                // during attack we don't move; AttackThenKill coroutine handles the rest
                Move(Vector2.zero);
                break;
        }
    }

    private float patrolDir = 1f;
    private void PatrolBehaviour()
    {
        // simple back-and-forth on X within patrolRange
        Vector3 pos = transform.position;
        float left = spawnPosition.x - patrolRange;
        float right = spawnPosition.x + patrolRange;
        if (pos.x <= left) patrolDir = 1f;
        else if (pos.x >= right) patrolDir = -1f;

        Move(new Vector2(patrolDir * patrolSpeed, 0f));
        FlipTowards(transform.position.x + patrolDir);
    }

    private void Move(Vector2 velocity)
    {
        // don't move while attacking
        if (isAttacking)
        {
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
            return;
        }
        // check obstacle ahead before moving
        float dirSign = Mathf.Sign(velocity.x);
        if (Mathf.Abs(velocity.x) > 0.01f)
        {
            if (IsObstacleAhead(dirSign))
            {
                // if patrolling, reverse direction; if chasing, stop
                if (state == State.Patrol)
                {
                    patrolDir = -patrolDir;
                }
                // stop horizontal movement against obstacle
                velocity.x = 0f;
            }
        }

        if (rb != null)
        {
            // smooth towards target horizontal velocity
            float targetX = velocity.x;
            float newX = Mathf.MoveTowards(rb.linearVelocity.x, targetX, accel * Time.deltaTime);
            rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            // use actual rigidbody velocity to determine running state
            float actualSpeed = Mathf.Abs(rb.linearVelocity.x);
            bool isRunning = actualSpeed > 0.01f;
            if (animator != null) animator.SetBool(runningParam, isRunning);
        }
        else
        {
            // move transform directly and infer movement from velocity input
            transform.position += (Vector3)(velocity * Time.deltaTime);
            bool isRunning = Mathf.Abs(velocity.x) > 0.01f;
            if (animator != null) animator.SetBool(runningParam, isRunning);
        }
    }

    private void FlipTowards(float worldX)
    {
        Vector3 s = transform.localScale;
        if (worldX < transform.position.x) s.x = -Mathf.Abs(s.x);
        else s.x = Mathf.Abs(s.x);
        transform.localScale = s;
    }

    private bool IsObstacleAhead(float dirSign)
    {
        if (Mathf.Approximately(dirSign, 0f)) return false;
        Vector2 origin = (Vector2)transform.position;
        // cast slightly ahead at waist height
        float distance = Mathf.Max(0.1f, Mathf.Abs(dirSign) * 0.5f + 0.1f);
        RaycastHit2D hit = Physics2D.CircleCast(origin, obstacleCheckRadius, new Vector2(dirSign, 0f), distance, obstacleMask);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;
        Vector2 dir = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance, lineOfSightMask);
        if (hit.collider == null) return true;
        return hit.collider.CompareTag("Player");
    }

    private bool IsGroundAhead(float dirSign)
    {
        if (Mathf.Approximately(dirSign, 0f)) return true;
        float forward = 0.5f + obstacleCheckRadius;
        Vector2 origin = (Vector2)transform.position + new Vector2(dirSign * forward, 0f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundMask);
        return hit.collider != null && !hit.collider.isTrigger;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            EnsureRespawnRef();
            if (!isAttacking) StartCoroutine(AttackThenKill());
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            EnsureRespawnRef();
            if (!isAttacking) StartCoroutine(AttackThenKill());
        }
    }

    private IEnumerator AttackThenKill()
    {
        isAttacking = true;
        // stop running animation
        if (animator != null) animator.SetBool(runningParam, false);
        // trigger attack animation
        if (animator != null && !string.IsNullOrEmpty(attackTrigger)) animator.SetTrigger(attackTrigger);

        float delay = attackFallbackDelay;
        if (!string.IsNullOrEmpty(attackClipName) && animator != null)
        {
            var controller = animator.runtimeAnimatorController;
            if (controller != null)
            {
                foreach (var clip in controller.animationClips)
                {
                    if (clip != null && clip.name == attackClipName)
                    {
                        delay = clip.length;
                        break;
                    }
                }
            }
        }

        yield return new WaitForSeconds(delay);
        EnsureRespawnRef();
        respawnManager?.KillPlayer();
        // clear cached player reference so we pick up the respawned player
        player = null;
        // return to idle/patrol so the enemy can detect the player again
        state = patrolWhenIdle ? State.Patrol : State.Idle;
        isAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}
