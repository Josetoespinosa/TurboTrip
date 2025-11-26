using UnityEngine;

public class PlayerNoclipMovement : MonoBehaviour
{
    public float acceleration = 15f;
    public float maxSpeed = 6f;
    public float friction = 10f;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool noclip;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetNoclip(bool enabled)
    {
        noclip = enabled;

        if (noclip)
        {
            rb.gravityScale = 5f;
            rb.linearVelocity = Vector2.zero;
            rb.linearDamping = 5f;
        }
        else
        {
            rb.gravityScale = 1f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
        }
    }

    void Update()
    {
        if (!noclip) return;

        var k = UnityEngine.InputSystem.Keyboard.current;

        input = Vector2.zero;

        if (k.wKey.isPressed) input.y = 1;
        if (k.sKey.isPressed) input.y = -1;
        if (k.aKey.isPressed) input.x = -1;
        if (k.dKey.isPressed) input.x = 1;

        input = input.normalized;
    }

    void FixedUpdate()
    {
        if (!noclip) return;

        // Si hay input → acelerar hacia la dirección
        if (input != Vector2.zero)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, input * maxSpeed, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // No hay input → fricción manual
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, friction * Time.fixedDeltaTime);
        }
    }
}
