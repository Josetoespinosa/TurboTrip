using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Jump2D : MonoBehaviour
{
    [Header("Salto")]
    public float JumpImpulse = 4f;
    public float JumpHoldForce = 7f;
    public float MaxJumpHoldTime = 0.75f;

    [Header("Dash + Salto (interacción)")]
    public float DashJumpWindow = 1f;
    [Range(0f, 1f)] public float DashHoldFactor = 0.5f;

    [Header("Referencias")]
    public PlayerInputSimple input;
    public GroundCheck2D groundCheck;
    public Movement2D movement;
    public AbilitySystem abilities;
    public PlayerSfx sfx;

    // estado
    float jumpHoldTimer;
    float currentMaxHold;
    bool jumping;
    bool canDouble;   // se resetea al tocar suelo
    float lastDashTime = -999f;

    Rigidbody2D rb;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        currentMaxHold = MaxJumpHoldTime;
    }

    void Update()
    {
        bool grounded = groundCheck && groundCheck.grounded;

        if (grounded) canDouble = true;

        // salto suelo (siempre disponible)
        if (input && input.jumpPressed && grounded)
            StartJump();

        // doble salto si desbloqueado
        if (input && input.jumpPressed && !grounded && canDouble && abilities && abilities.Has(AbilitySystem.Ability.DoubleJump))
        {
            StartJump();
            canDouble = false;
            sfx?.PlayDoubleJump();
        }

        // hold
        if (jumping && input && input.jumpHeld && jumpHoldTimer < currentMaxHold)
            jumpHoldTimer += Time.deltaTime;

        if (jumping && (input.jumpReleased || jumpHoldTimer >= currentMaxHold))
            jumping = false;
    }

    void FixedUpdate()
    {
        if (jumping && jumpHoldTimer < currentMaxHold)
        {
            float vy = rb.linearVelocity.y + (JumpHoldForce * Time.fixedDeltaTime);
            movement.SetVerticalVelocity(vy);
        }
    }

    void StartJump()
    {
        sfx?.PlayJump();

        var v = rb.linearVelocity; v.y = 0f; rb.linearVelocity = v;
        rb.AddForce(Vector2.up * JumpImpulse, ForceMode2D.Impulse);

        bool withinDash = (Time.time - lastDashTime) <= DashJumpWindow;
        currentMaxHold = withinDash ? MaxJumpHoldTime * DashHoldFactor : MaxJumpHoldTime;

        movement.SetHorizontalVelocity(rb.linearVelocity.x);

        jumping = true;
        jumpHoldTimer = 0f;
    }

    // llamado por Dash2D
    public void NotifyDashStarted()
    {
        lastDashTime = Time.time;
    }
}
