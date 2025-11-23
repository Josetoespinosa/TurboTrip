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
    public float loseFactor = 1.5f;

    [Header("Movement")]
    public float chaseSpeed = 3.5f;
    public float patrolSpeed = 1.5f;
    public bool patrolWhenIdle = true;
    public float patrolRange = 3f; // horizontal range from spawn when patrolling
    [Tooltip("Layers considered obstacles for the enemy (walls, ground, platforms)")]
    public LayerMask obstacleMask = ~0;
    [Tooltip("Radius used for obstacle checks (circlecast)")]
    public float obstacleCheckRadius = 0.2f;

    // references
    private Transform player;
    private Rigidbody2D rb;
    private Vector3 spawnPosition;
    private bool chasing = false;
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

    // respawn manager reference to call KillPlayer
    private PlayerRespawnManager respawnManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spawnPosition = transform.position;
        EnsureRespawnRef();
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

        if (!chasing)
        {
            if (dist <= detectionRadius)
            {
                chasing = true;
            }
            else
            {
                // patrol when idle
                if (patrolWhenIdle) PatrolBehaviour();
                else
                {
                    if (animator != null) animator.SetBool(runningParam, false);
                }
            }
        }
        else
        {
            // if player too far, stop chasing
            if (dist > detectionRadius * loseFactor)
            {
                chasing = false;
                return;
            }

            // move towards player horizontally
            Vector2 dir = (player.position - transform.position);
            Vector2 move = new Vector2(Mathf.Sign(dir.x), 0f);
            Move(move * chaseSpeed);

            // flip sprite if any
            FlipTowards(player.position.x);

            // if close enough, perform attack (then kill)
            if (dist <= catchDistance)
            {
                EnsureRespawnRef();
                if (!isAttacking)
                {
                    StartCoroutine(AttackThenKill());
                }
            }
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
        // check obstacle ahead before moving
        float dirSign = Mathf.Sign(velocity.x);
        if (Mathf.Abs(velocity.x) > 0.01f)
        {
            if (IsObstacleAhead(dirSign))
            {
                // if patrolling, reverse direction; if chasing, stop
                if (!chasing)
                {
                    patrolDir = -patrolDir;
                }
                // stop horizontal movement against obstacle
                velocity.x = 0f;
            }
        }

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(velocity.x, rb.linearVelocity.y);
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
