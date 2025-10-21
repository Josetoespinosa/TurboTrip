using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Nick : MonoBehaviour
{
    // ===== MOVIMIENTO / INERCIA =====
    [Header("Movimiento")]
    public float BaseMaxSpeed = 5f;
    public float BoostedMaxSpeed = 6f;
    public float TimeToMaxBoost = 1.5f;
    public float Acceleration = 40f;

    [Header("Inercia")]
    public float GroundFriction = 10f;
    public float AirFriction = 6f;
    [Range(0f, 1f)] public float TurnResistance = 0.6f;
    [Range(0f, 1f)] public float AirControl = 0.7f;

    // ===== SALTO =====
    [Header("Salto")]
    public float JumpImpulse = 4f;
    public float JumpHoldForce = 7f;
    public float MaxJumpHoldTime = 0.75f;

    // Limitar “dash-jump alto” (Opción C)
    [Tooltip("Ventana para considerar interacción dash + salto")]
    public float DashJumpWindow = 1f;
    [Tooltip("Proporción del tiempo de hold permitido si salto ocurre dentro de la ventana de dash")]
    [Range(0f, 1f)] public float DashHoldFactor = 0.5f;

    // ===== DASH =====
    [Header("Dash")]
    [Tooltip("Velocidad X instantánea durante el dash")]
    public float DashSpeed = 13f;
    public float DashDuration = 0.2f;
    public float DashCooldown = 1f;
    [Tooltip("Porcentaje de la velocidad del dash que se conserva como momentum al terminar")]
    [Range(0f, 1f)] public float DashCarryOver = 0.5f;

    // ===== SUELO / PAREDES =====
    [Header("Suelo / Pared")]
    public float GroundRayDistance = 0.2f;
    public LayerMask GroundMask = ~0;

    [Tooltip("Raycast lateral para detectar pared delante")]
    public float WallProbeDistance = 0.1f;
    public Vector2 WallProbeOffset = new Vector2(0f, 0.05f);
    [Tooltip("Si la vel. es menor a esto y estás contra pared, al invertir input liberamos momentum")]
    public float AntiStickMinSpeed = 0.3f;

    [Header("VFX")]
    public ParticleSystem doubleJumpParticles;

    // ===== Componentes =====
    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // ===== Estado =====
    private float inputDir;                // -1, 0, 1
    private float sameDirTimer = 0f;
    private int lastDirSign = 0;

    private bool grounded;
    private bool jumping;
    private bool hasDoublejump;
    private bool hasDash;
    private bool canDash = true;
    private bool isDashing = false;
    private float lastMoveDir = 1f;

    private float jumpHoldTimer = 0f;
    private float currentMaxJumpHold;      // puede reducirse por dash-jump
    private float lastDashTime = -999f;
    private float originalGravity;

    private Vector2 momentum = Vector2.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentMaxJumpHold = MaxJumpHoldTime;
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        // ---- INPUT HORIZONTAL ----
        inputDir = 0f;
        if (Keyboard.current.aKey.isPressed) inputDir -= 1f;
        if (Keyboard.current.dKey.isPressed) inputDir += 1f;
        if (inputDir != 0) lastMoveDir = inputDir;

        // Animación + flip
        if (animator) animator.SetBool("Running", Mathf.Abs(inputDir) > 0.01f);
        if (inputDir < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (inputDir > 0f) transform.localScale = new Vector3(1f, 1f, 1f);

        // ---- SUELO ----
        Debug.DrawRay(transform.position, Vector2.down * GroundRayDistance, Color.red);
        grounded = Physics2D.Raycast(transform.position, Vector2.down, GroundRayDistance, GroundMask);
        if (animator) animator.SetBool("Jumping", !grounded);

        // ---- SALTO (suelo) ----
        if (Keyboard.current.spaceKey.wasPressedThisFrame && grounded)
        {
            StartJump();
        }

        // ---- DOBLE SALTO ----
        if (Keyboard.current.spaceKey.wasPressedThisFrame && hasDoublejump && !grounded)
        {
            StartJump();
            hasDoublejump = false;
            if (doubleJumpParticles) doubleJumpParticles.Play();
        }

        // ---- HOLD DEL SALTO ----
        if (jumping && Keyboard.current.spaceKey.isPressed && jumpHoldTimer < currentMaxJumpHold)
            jumpHoldTimer += Time.deltaTime;

        if (jumping && (Keyboard.current.spaceKey.wasReleasedThisFrame || jumpHoldTimer >= currentMaxJumpHold))
            jumping = false;

        // ---- DASH ----
        if (Keyboard.current.shiftKey.wasPressedThisFrame && canDash && hasDash)
            StartCoroutine(DashRoutine(lastMoveDir));

        // Reset de habilidades al tocar suelo
        if (grounded)
        {
            hasDoublejump = true;
            hasDash = true;
        }

        // ---- BOOST por mantener dirección ----
        int currentSign = Mathf.RoundToInt(Mathf.Sign(inputDir));
        if (currentSign != 0)
        {
            if (currentSign == lastDirSign) sameDirTimer += Time.deltaTime;
            else { sameDirTimer = 0f; lastDirSign = currentSign; }
        }
        else
        {
            sameDirTimer = Mathf.Max(0f, sameDirTimer - Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // Si estamos dashing, imponemos velocidad y salimos (sin clamps/fricción)
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(momentum.x, 0f);
            return;
        }

        int moveSign = 0;
        if (Mathf.Abs(momentum.x) > 0.01f) moveSign = (int)Mathf.Sign(momentum.x);
        else if (Mathf.Abs(inputDir) > 0.01f) moveSign = (int)Mathf.Sign(inputDir);

        if (moveSign != 0 && IsTouchingWall(moveSign))
        {
            // Si el momentum empuja hacia la pared, lo cortamos
            if (Mathf.Sign(momentum.x) == moveSign)
                momentum.x = 0f;

            if (Mathf.Abs(rb.linearVelocity.x) < AntiStickMinSpeed && Mathf.Sign(inputDir) == -moveSign)
                momentum.x = 0f;
        }

        float t = Mathf.Clamp01(TimeToMaxBoost > 0f ? (sameDirTimer / TimeToMaxBoost) : 1f);
        float maxSpeedNow = Mathf.Lerp(BaseMaxSpeed, BoostedMaxSpeed, t);

        float accel = Acceleration;
        if (!grounded) accel *= AirControl;

        float friction = grounded ? GroundFriction : AirFriction;

        // ---- Aceleración desde input sobre momentum ----
        float desiredAccel = inputDir * accel;

        // Resistir giro contra momentum
        if (Mathf.Abs(inputDir) > 0.01f && Mathf.Abs(momentum.x) > 0.0001f && Mathf.Sign(inputDir) != Mathf.Sign(momentum.x))
            desiredAccel *= (1f - TurnResistance);

        momentum.x += desiredAccel * dt;

        if (Mathf.Abs(inputDir) < 0.01f)
        {
            momentum.x = Mathf.MoveTowards(momentum.x, 0f, friction * dt);
        }
        else
        {
            float lightFriction = 0.25f * friction * dt;
            if (Mathf.Abs(momentum.x) > 0f)
                momentum.x = Mathf.MoveTowards(momentum.x, momentum.x + Mathf.Sign(momentum.x) * 0.0001f, lightFriction);
        }

        momentum.x = Mathf.Clamp(momentum.x, -maxSpeedNow, maxSpeedNow);

        float vy = rb.linearVelocity.y;
        if (jumping && jumpHoldTimer < currentMaxJumpHold)
            vy += (JumpHoldForce * dt);

        rb.linearVelocity = new Vector2(momentum.x, vy);
    }

    private void StartJump()
    {
        bool withinDashWindow = (Time.time - lastDashTime) <= DashJumpWindow;

        if (isDashing) EndDashImmediately();

        Vector2 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector2.up * JumpImpulse, ForceMode2D.Impulse);

        // Definir la ventana de hold para este salto (cap si hubo dash reciente)
        currentMaxJumpHold = withinDashWindow ? MaxJumpHoldTime * DashHoldFactor : MaxJumpHoldTime;

        momentum.x = rb.linearVelocity.x; // mantener coherencia horizontal
        jumping = true;
        jumpHoldTimer = 0f;
    }

    private IEnumerator DashRoutine(float direction)
    {
        canDash = false;
        hasDash = false;
        isDashing = true;
        lastDashTime = Time.time;
        if (spriteRenderer) spriteRenderer.color = Color.grey;

        float savedGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashVX = direction * DashSpeed;
        momentum.x = dashVX;
        rb.linearVelocity = new Vector2(dashVX, 0f);

        yield return new WaitForSeconds(DashDuration);

        rb.gravityScale = savedGravity;
        isDashing = false;

        // Conservar parte del dash como inercia
        momentum.x = rb.linearVelocity.x * DashCarryOver;

        yield return new WaitForSeconds(DashCooldown);
        canDash = true;
        if (spriteRenderer) spriteRenderer.color = Color.white;
    }

    private void EndDashImmediately()
    {
        isDashing = false;
        rb.gravityScale = originalGravity;
        if (spriteRenderer) spriteRenderer.color = Color.white;
    }

    private bool IsTouchingWall(int dirSign)
    {
        if (capsuleCollider == null) return false;

        float halfWidth = capsuleCollider.size.x * 0.5f * Mathf.Abs(transform.localScale.x);
        Vector2 origin = (Vector2)transform.position + new Vector2(dirSign * (halfWidth + 0.01f), WallProbeOffset.y);

        RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(dirSign, 0f), WallProbeDistance, GroundMask);
        Debug.DrawRay(origin, new Vector2(dirSign, 0f) * WallProbeDistance, Color.cyan);
        return hit.collider != null;
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        // Si chocas fuerte contra cualquier colisionable en GroundMask, corta momentum hacia esa normal
        if (((1 << collision.collider.gameObject.layer) & GroundMask) != 0)
        {
            float impactSpeed = rb.linearVelocity.magnitude;
            if (impactSpeed > 10f)
            {
                Vector2 normal = collision.contacts[0].normal;
                // Anula componente del momentum hacia la pared
                float proj = Vector2.Dot(momentum, -normal);
                if (proj > 0f) momentum -= (-normal) * proj;
            }
        }
    }*/
}
