using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Dash2D : MonoBehaviour
{
    [Header("Dash")]
    public float DashSpeed = 10f;
    public float DashDuration = 0.2f;
    public float DashCooldown = 1f;
    [Range(0f, 1f)] public float DashCarryOver = 0.5f;

    [Header("Referencias")]
    public PlayerInputSimple input;
    public Movement2D movement;
    public AbilitySystem abilities;
    public Jump2D jump2D;
    public PlayerSfx sfx;
    public SpriteRenderer spriteRenderer;

    bool canDash = true;
    float savedGravity;
    public bool isDashing = false;
    Rigidbody2D rb;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (input && input.dashPressed && canDash && abilities && abilities.Has(AbilitySystem.Ability.Dash))
        {
            float dir = Mathf.Abs(input.moveAxis) < 0.1f ? Mathf.Sign(transform.localScale.x) : Mathf.Sign(input.moveAxis);
            StartCoroutine(DashRoutine(dir));
        }
    }

    IEnumerator DashRoutine(float direction)
    {
        canDash = false;
        isDashing = true;
        movement.ignoreMaxSpeed = true;
        jump2D?.NotifyDashStarted();

        sfx?.PlayDash();
        var sr = spriteRenderer; if (sr) sr.color = Color.gray;

        savedGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashVX = direction * DashSpeed;
        movement.SetHorizontalVelocity(dashVX);
        rb.linearVelocity = new Vector2(dashVX, 0f);

        yield return new WaitForSeconds(DashDuration);

        rb.gravityScale = savedGravity;
        isDashing = false;
        movement.ignoreMaxSpeed = false;

        float carry = rb.linearVelocity.x * DashCarryOver;
        movement.SetHorizontalVelocity(carry);

        yield return new WaitForSeconds(DashCooldown);
        canDash = true;
        if (sr) sr.color = Color.white;
    }
}
