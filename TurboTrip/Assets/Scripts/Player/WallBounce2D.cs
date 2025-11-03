using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class WallBounce2D : MonoBehaviour
{
    [Header("Umbrales")]
    public float hitSpeedThreshold = 8f;   // para SFX fuerte
    public float dashLikeThresholdFactor = 0.9f; // relativo a la vel. de dash

    [Header("Referencias")]
    public Rigidbody2D rb;
    public Movement2D movement;
    public Dash2D dash;
    public PlayerSfx sfx;

    Vector2 preCollisionVelocity;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        preCollisionVelocity = rb.linearVelocity;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 bestNormal = Vector2.zero;
        float bestDot = -1f;

        foreach (ContactPoint2D c in collision.contacts)
        {
            float dot = Mathf.Abs(c.normal.x);
            if (dot > bestDot) { bestDot = dot; bestNormal = c.normal; }
        }

        // si no es casi normal horizontal, salimos
        if (Mathf.Abs(bestNormal.x) < 0.3f) return;

        float horizontalSpeed = Mathf.Abs(preCollisionVelocity.x);
        bool strongBounce = horizontalSpeed > (dash ? dash.DashSpeed * dashLikeThresholdFactor : hitSpeedThreshold);

        float bounceX = -preCollisionVelocity.x;
        if (strongBounce)
        {
            bounceX *= 1.2f;
            sfx?.PlayWallHit();
        }
        else
        {
            bounceX = Mathf.Sign(bounceX) * Mathf.Max(Mathf.Abs(bounceX), 2f);
        }

        rb.linearVelocity = new Vector2(bounceX, rb.linearVelocity.y);
        if (movement) movement.momentum = rb.linearVelocity;
    }
}
