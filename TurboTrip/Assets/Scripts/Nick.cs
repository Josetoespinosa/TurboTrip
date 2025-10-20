using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Nick : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad máxima base al empezar a correr")]
    public float BaseMaxSpeed = 5f;
    [Tooltip("Velocidad máxima cuando mantienes dirección por un tiempo")]
    public float BoostedMaxSpeed = 8f;
    [Tooltip("Segundos necesarios manteniendo misma dirección para llegar al boost máximo")]
    public float TimeToMaxBoost = 1.5f;
    [Tooltip("Aceleración horizontal (u/s^2)")]
    public float Acceleration = 40f;
    [Tooltip("Desaceleración cuando sueltas las teclas (u/s^2)")]
    public float Deceleration = 50f;

    [Header("Salto (altura variable con Space)")]
    [Tooltip("Impulso inicial del salto")]
    public float JumpImpulse = 4f;
    [Tooltip("Fuerza vertical adicional mientras se mantiene Space")]
    public float JumpHoldForce = 7f;
    [Tooltip("Máximo tiempo que se puede seguir 'cargando' el salto")]
    public float MaxJumpHoldTime = 0.25f;

    [Header("Dash")]
    [Tooltip("Impulso del dash")]
    public float DashImpulse = 15f;
    [Tooltip("Cooldown del dash")]
    public float DashCooldown = 1f;
    [Tooltip("Duracion del dash")]
    public float DashDuration = 0.2f;

    [Header("Suelo")]
    public float GroundRayDistance = 0.2f;
    public LayerMask GroundMask = ~0; // por si quieres filtrar capas de suelo

    public ParticleSystem doubleJumpParticles;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float inputDir;                // -1, 0, 1 según A/D
    private float sameDirTimer = 0f;       // tiempo manteniendo misma dirección
    private int lastDirSign = 0;           // para detectar cambio de dirección

    private bool grounded;
    private bool jumping;
    private bool hasDoublejump;
    private bool hasDash;
    private bool canDash = true;
    private bool isDashing = false;
    private float lastMoveDir = 1f;
    private float jumpHoldTimer = 0f;

    void Start()
    {
        Debug.Log("Nick has been initialized.");
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>(); 
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // --- INPUT HORIZONTAL ---
        inputDir = 0f;
        if (Keyboard.current.aKey.isPressed) inputDir -= 1f;
        if (Keyboard.current.dKey.isPressed) inputDir += 1f;

        if (inputDir != 0)
            lastMoveDir = inputDir;

        // Animación correr + flip
        animator.SetBool("Running", Mathf.Abs(inputDir) > 0.01f);
        if (inputDir < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (inputDir > 0f) transform.localScale = new Vector3(1f, 1f, 1f);

        // --- CHEQUEO SUELO ---
        Debug.DrawRay(transform.position, Vector2.down * GroundRayDistance, Color.red);
        grounded = Physics2D.Raycast(transform.position, Vector2.down, GroundRayDistance, GroundMask);

        // --- SALTO: inicio al presionar Space (solo si en suelo) ---
        if (Keyboard.current.spaceKey.wasPressedThisFrame && grounded)
        {
            StartJump();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && hasDoublejump && !grounded)
        {
            StartJump();
            hasDoublejump = false;

            if (doubleJumpParticles != null)
            {
                doubleJumpParticles.Play();
            }
        }

        // --- SALTO: mantener Space para más altura, con límite ---
        if (jumping && Keyboard.current.spaceKey.isPressed && jumpHoldTimer < MaxJumpHoldTime)
        {
            // aplicamos una fuerza ascendente suave por FixedUpdate, aquí sólo acumulamos tiempo
            jumpHoldTimer += Time.deltaTime;
        }
        // si se suelta Space o llega al límite, se acaba la fase de hold
        if (jumping && (Keyboard.current.spaceKey.wasReleasedThisFrame || jumpHoldTimer >= MaxJumpHoldTime))
        {
            jumping = false;
        }

        animator.SetBool("Jumping", !grounded);

        //Dash 

        if (Keyboard.current.shiftKey.wasPressedThisFrame && canDash && hasDash)
        {
            StartCoroutine(DashRoutine(lastMoveDir));
        }

        if (grounded)
        {
            hasDoublejump = true;
            hasDash = true;

        }

        // --- LÓGICA DE BOOST TEMPORAL DE VELOCIDAD MÁXIMA ---
        int currentSign = Mathf.RoundToInt(Mathf.Sign(inputDir));
        if (currentSign != 0)
        {
            if (currentSign == lastDirSign)
            {
                sameDirTimer += Time.deltaTime; // seguimos sumando tiempo en la misma dirección
            }
            else
            {
                // Cambio de dirección → reiniciamos el temporizador para el boost
                sameDirTimer = 0f;
                lastDirSign = currentSign;
            }
        }
        else
        {
            // sin input: no seguimos acumulando, pero tampoco lo reseteamos del todo (se siente mejor)
            sameDirTimer = Mathf.Max(0f, sameDirTimer - Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        // --- CÁLCULO DE VELOCIDAD MÁXIMA SEGÚN TIEMPO MANTENIDO ---
        float t = Mathf.Clamp01(TimeToMaxBoost > 0f ? (sameDirTimer / TimeToMaxBoost) : 1f);
        float maxSpeedNow = Mathf.Lerp(BaseMaxSpeed, BoostedMaxSpeed, t);

        // --- ACELERACIÓN / DESCELERACIÓN CON INERCIA SENCILLA ---
        float targetSpeed = inputDir * maxSpeedNow;
        float vx = rb.linearVelocity.x;

        // Si hay input, aceleramos hacia target; si no, desaceleramos hacia 0
        if (Mathf.Abs(inputDir) > 0.01f)
        {
            vx = Mathf.MoveTowards(vx, targetSpeed, Acceleration * Time.fixedDeltaTime);
        }
        else
        {
            vx = Mathf.MoveTowards(vx, 0f, Deceleration * Time.fixedDeltaTime);
        }

        // --- SALTO: aplicar fuerza de hold mientras dure la ventana ---
        float vy = rb.linearVelocity.y;
        if (jumping && jumpHoldTimer < MaxJumpHoldTime)
        {
            vy += (JumpHoldForce * Time.fixedDeltaTime);
        }

        rb.linearVelocity = new Vector2(vx, vy);
    }

    private void StartJump()
    {
        // Reiniciar componente vertical y dar impulso inicial
        Vector2 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        rb.AddForce(Vector2.up * JumpImpulse, ForceMode2D.Impulse);

        jumping = true;
        jumpHoldTimer = 0f;
        Debug.Log("Nick is jumping.");
    }

    // Realizamos Dash y pausamos gravedad
    private IEnumerator DashRoutine(float direction)
    {
        canDash = false;
        hasDash = false;
        isDashing = true;
        UpdateSprite();

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(0f, 0f);

        rb.AddForce(Vector2.right * direction * DashImpulse, ForceMode2D.Impulse);

        yield return new WaitForSeconds(DashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(DashCooldown);
        canDash = true;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        spriteRenderer.color = canDash ? Color.white : Color.yellow;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            float impactSpeed = rb.linearVelocity.magnitude;

            if (impactSpeed > 10f)
            {
                Vector2 recoilDirection = (transform.position - collision.transform.position).normalized;

                float recoilForce = 10f;
                rb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);

                CameraShake.Instance.Shake(0.2f, 0.3f);
            }

        }
    }
}
