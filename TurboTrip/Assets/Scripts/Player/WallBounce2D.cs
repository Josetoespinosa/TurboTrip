using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class WallBounce2D : MonoBehaviour
{
    [Header("Umbrales")]
    public float minBounceSpeed = 3f;
    public float strongHitMultiplier = 1.1f; 
    public float strongDashFactor = 0.8f;
    public float controlLockTime = 0.12f;
    public float wallBounceFactor = 0.05f;

    [Header("Referencias")]
    public Rigidbody2D rb;
    public Movement2D movement;
    public Dash2D dash;
    public PlayerSfx sfx;

    private Vector2 preCollisionVelocity;

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
        if (!movement) return;

        if (!dash || !dash.isDashing)
            return;

        Vector2 bestNormal = Vector2.zero;
        float bestDot = -1f;

        foreach (var contact in collision.contacts)
        {
            float dot = Mathf.Abs(contact.normal.x);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestNormal = contact.normal;
            }
        }

        if (Mathf.Abs(bestNormal.x) < 0.1f)
            return;

        float horizSpeed = Mathf.Abs(preCollisionVelocity.x);
        float dashSpeed = dash ? dash.DashSpeed : 12f;
        bool strongHit = horizSpeed > dashSpeed * strongDashFactor;

        if (horizSpeed < minBounceSpeed)
            return;

        float bounceX = -preCollisionVelocity.x * wallBounceFactor;
        if (strongHit)
        {
            bounceX *= strongHitMultiplier;
            sfx?.PlayWallHit();
        }

        rb.linearVelocity = new Vector2(bounceX, rb.linearVelocity.y);
        movement.momentum = rb.linearVelocity;

        movement.ignoreMaxSpeed = true;
        StartCoroutine(UnlockMovementAfterDelay());
    }


    IEnumerator UnlockMovementAfterDelay()
    {
        yield return new WaitForSeconds(controlLockTime);
        movement.ignoreMaxSpeed = false;
    }
}
