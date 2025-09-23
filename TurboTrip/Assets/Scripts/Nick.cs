using UnityEngine;
using UnityEngine.InputSystem;

public class Nick : MonoBehaviour
{
    public float Speed = 50;
    public float JumpForce = 15;
    private Rigidbody2D Rigidbody2D;
    private Animator animator;
    private float Horizontal;
    private bool Grounded;





    void Start()
    {
        Debug.Log("Nick has been initialized.");
        Rigidbody2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Horizontal = 0f;
        if (Keyboard.current.aKey.isPressed)
        {
            Horizontal -= 10f;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            Horizontal += 10f;
        }

        animator.SetBool("Running", Horizontal != 0.0f);

        if (Horizontal < 0.0f) transform.localScale = new Vector3(-1f, 1f, 1f);
        else if (Horizontal > 0.0f) transform.localScale = new Vector3(1f, 1f, 1f);

        Debug.DrawRay(transform.position, Vector2.down * 0.4f, Color.red);
        if (Physics2D.Raycast(transform.position, Vector2.down, 0.4f))
        {
            Grounded = true;
        }
        else
        {
            Grounded = false;
        }

        if (Keyboard.current.wKey.isPressed && Grounded)
        {
            Jump();

        }

        animator.SetBool("Jumping", !Grounded);

    }

    private void Jump()
    {
        Debug.Log("Nick is jumping.");
        Rigidbody2D.AddForce(Vector2.up * JumpForce);
    }

    private void FixedUpdate()
    {
        Rigidbody2D.linearVelocity = new Vector2(Horizontal, Rigidbody2D.linearVelocity.y);
    }
}
