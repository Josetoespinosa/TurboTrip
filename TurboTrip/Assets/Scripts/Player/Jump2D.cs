using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class Jump2D : MonoBehaviour
{
    [Header("Salto")]
    public float JumpImpulse = 4f;
    public float JumpHoldForce = 7f;
    public float MaxJumpHoldTime = 0.75f;

    [Header("Dash + Salto (interacci�n)")]
    public float DashJumpWindow = 1f;
    [Range(0f, 1f)] public float DashHoldFactor = 0.5f;

    [Header("VFX (opcional)")]
    public ParticleSystem doubleJumpFx;

    [Header("Animation (optional)")]
    [Tooltip("Animator boolean parameter name to set true when performing a double jump; leave empty to disable")]
    public string doubleJumpBoolName = "DoubleJump";
    [Tooltip("Optional animator trigger name to fire for double jump; if set this will be used instead of the bool")]
    public string doubleJumpTriggerName = "";
    [Tooltip("Optional name of the double-jump animation clip. If set, its length will be used to auto-clear the parameter. Otherwise the fallback duration is used.")]
    public string doubleJumpClipName = "";
    [Tooltip("Fallback duration (seconds) to wait before clearing double-jump param when clip length can't be determined")]
    public float doubleJumpFallbackDuration = 0.6f;

    [Header("Referencias (autocables)")]
    public PlayerInputSimple input;
    public GroundCheck2D groundCheck;
    public Movement2D movement;
    public AbilitySystem abilities;
    public PlayerSfx sfx;

    float jumpHoldTimer;
    float currentMaxHold;
    bool jumping;
    bool canDouble;
    float lastDashTime = -999f;

    Rigidbody2D rb;
    Coroutine autoClearRoutine;

    void Awake()
    {
        rb ??= GetComponent<Rigidbody2D>();
        input ??= GetComponent<PlayerInputSimple>();
        groundCheck ??= GetComponent<GroundCheck2D>();
        movement ??= GetComponent<Movement2D>();
        abilities ??= GetComponent<AbilitySystem>();
        sfx ??= GetComponent<PlayerSfx>();
        currentMaxHold = MaxJumpHoldTime;

        // Deja el VFX apagado de inicio
        if (doubleJumpFx) doubleJumpFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void Update()
    {
        bool grounded = groundCheck && groundCheck.grounded;
        if (grounded)
        {
            canDouble = true;
            // landing should clear any double-jump suppression
            if (movement != null) movement.suppressJumping = false;
        }

        if (input && input.jumpPressed && grounded)
            StartJump();

        // doble salto si est� desbloqueado
        if (input && input.jumpPressed && !grounded && canDouble && abilities && abilities.Has(AbilitySystem.Ability.DoubleJump))
        {
            StartJump();
            canDouble = false;
            sfx?.PlayDoubleJump();
            if (doubleJumpFx) doubleJumpFx.Play();

            // Trigger animator double-jump parameter on Movement2D animator (if present)
            var anim = movement != null ? movement.animator : null;
            if (anim != null)
            {
                // Prefer trigger if configured
                if (!string.IsNullOrEmpty(doubleJumpTriggerName))
                    anim.SetTrigger(doubleJumpTriggerName);
                else if (!string.IsNullOrEmpty(doubleJumpBoolName))
                    anim.SetBool(doubleJumpBoolName, true);
                // Suppress regular Jumping bool while double-jump plays
                anim.SetBool("Jumping", false);
                if (movement != null) movement.suppressJumping = true;

                // Start or restart auto-clear coroutine
                if (autoClearRoutine != null) StopCoroutine(autoClearRoutine);
                autoClearRoutine = StartCoroutine(AutoClearDoubleJumpParam(anim));
            }
        }

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

    private IEnumerator AutoClearDoubleJumpParam(Animator anim)
    {
        float wait = Mathf.Max(0f, doubleJumpFallbackDuration);
        if (anim != null && !string.IsNullOrEmpty(doubleJumpClipName))
        {
            var ctrl = anim.runtimeAnimatorController;
            if (ctrl != null)
            {
                foreach (var clip in ctrl.animationClips)
                {
                    if (clip == null) continue;
                    if (clip.name.Equals(doubleJumpClipName, StringComparison.OrdinalIgnoreCase))
                    {
                        wait = clip.length;
                        break;
                    }
                }
            }
        }

        if (wait > 0f) yield return new WaitForSeconds(wait);
        else yield return null;

        if (anim != null)
        {
            if (!string.IsNullOrEmpty(doubleJumpBoolName))
                anim.SetBool(doubleJumpBoolName, false);
            // restore Jumping based on ground
            if (movement != null && movement.groundCheck != null)
                anim.SetBool("Jumping", !movement.groundCheck.grounded);
            else
                anim.SetBool("Jumping", true);
        }

        // Clear suppression so Movement2D can manage Jumping again
        if (movement != null) movement.suppressJumping = false;

        autoClearRoutine = null;
    }

    public void NotifyDashStarted() { lastDashTime = Time.time; }
}
