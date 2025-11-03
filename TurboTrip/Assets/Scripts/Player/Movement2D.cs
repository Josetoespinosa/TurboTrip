using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Movement2D : MonoBehaviour
{
    [Header("Movimiento / Inercia")]
    public float BaseMaxSpeed = 5f;
    public float BoostedMaxSpeed = 6f;
    public float TimeToMaxBoost = 1.5f;
    public float Acceleration = 40f;

    [Header("Inercia")]
    public float GroundFriction = 10f;
    public float AirFriction = 6f;
    [Range(0f, 1f)] public float TurnResistance = 0.6f;
    [Range(0f, 1f)] public float AirControl = 0.7f;

    [Header("Pared")]
    public float WallProbeDistance = 0.1f;
    public Vector2 WallProbeOffset = new Vector2(0f, 0.05f);
    public float AntiStickMinSpeed = 0.3f;
    public LayerMask GroundMask = ~0;

    [Header("Referencias")]
    public PlayerInputSimple input;
    public GroundCheck2D groundCheck;
    public CapsuleCollider2D capsuleCollider;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Lecturas (solo lectura)")]
    public Vector2 momentum;
    public float sameDirTimer;
    public int lastDirSign;

    Rigidbody2D rb;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Flip + anim básica
        if (input)
        {
            if (input.moveAxis < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
            else if (input.moveAxis > 0f) transform.localScale = new Vector3(1f, 1f, 1f);

            if (animator) animator.SetBool("Running", Mathf.Abs(input.moveAxis) > 0.01f);
        }
        if (animator && groundCheck) animator.SetBool("Jumping", !groundCheck.grounded);

        // Boost por mantener dirección
        int currentSign = Mathf.RoundToInt(Mathf.Sign(input ? input.moveAxis : 0f));
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

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        bool grounded = groundCheck && groundCheck.grounded;
        float inputAxis = input ? input.moveAxis : 0f;

        // pared frontal
        int moveSign = 0;
        if (Mathf.Abs(momentum.x) > 0.01f) moveSign = (int)Mathf.Sign(momentum.x);
        else if (Mathf.Abs(inputAxis) > 0.01f) moveSign = (int)Mathf.Sign(inputAxis);

        if (moveSign != 0 && IsTouchingWall(moveSign))
        {
            if (Mathf.Sign(momentum.x) == moveSign) momentum.x = 0f;
            if (Mathf.Abs(rb.linearVelocity.x) < AntiStickMinSpeed && Mathf.Sign(inputAxis) == -moveSign)
                momentum.x = 0f;
        }

        float t = Mathf.Clamp01(TimeToMaxBoost > 0f ? (sameDirTimer / TimeToMaxBoost) : 1f);
        float maxSpeedNow = Mathf.Lerp(BaseMaxSpeed, BoostedMaxSpeed, t);

        float accel = Acceleration * (grounded ? 1f : AirControl);
        float friction = grounded ? GroundFriction : AirFriction;

        float desiredAccel = inputAxis * accel;

        if (Mathf.Abs(inputAxis) > 0.01f && Mathf.Abs(momentum.x) > 0.0001f &&
            Mathf.Sign(inputAxis) != Mathf.Sign(momentum.x))
        {
            desiredAccel *= (1f - TurnResistance);
        }

        momentum.x += desiredAccel * dt;

        if (Mathf.Abs(inputAxis) < 0.01f)
            momentum.x = Mathf.MoveTowards(momentum.x, 0f, friction * dt);
        else
        {
            float lightF = 0.25f * friction * dt;
            if (Mathf.Abs(momentum.x) > 0f)
                momentum.x = Mathf.MoveTowards(momentum.x, momentum.x + Mathf.Sign(momentum.x) * 0.0001f, lightF);
        }

        momentum.x = Mathf.Clamp(momentum.x, -maxSpeedNow, maxSpeedNow);
        rb.linearVelocity = new Vector2(momentum.x, rb.linearVelocity.y);
    }

    public void SetHorizontalVelocity(float vx)
    {
        momentum.x = vx;
        rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
    }

    public void SetVerticalVelocity(float vy)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, vy);
    }

    bool IsTouchingWall(int dirSign)
    {
        if (!capsuleCollider) return false;
        float halfW = capsuleCollider.size.x * 0.5f * Mathf.Abs(transform.localScale.x);
        Vector2 origin = (Vector2)transform.position + new Vector2(dirSign * (halfW + 0.01f), WallProbeOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(origin, new Vector2(dirSign, 0f), WallProbeDistance, GroundMask);
        Debug.DrawRay(origin, new Vector2(dirSign, 0f) * WallProbeDistance, Color.cyan);
        return hit.collider != null;
    }
}
