using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class SawHazard : MonoBehaviour
{
    public enum MoveDirection { Horizontal, Vertical }

    [Header("Configuración")]
    public MoveDirection direction = MoveDirection.Horizontal;

    [Tooltip("Distancia que se moverá desde la posición inicial.")]
    public float range = 3f;

    [Tooltip("Velocidad de movimiento de la sierra.")]
    public float speed = 3f;

    private Rigidbody2D rb;
    private Collider2D col;

    private Vector2 startPos;
    private Vector2 targetPos;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        col.isTrigger = true;

        startPos = rb.position;

        // Primer objetivo
        if (direction == MoveDirection.Horizontal)
            targetPos = startPos + new Vector2(range, 0f);
        else
            targetPos = startPos + new Vector2(0f, range);
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;

        // Mover hacia target
        Vector2 newPos = Vector2.MoveTowards(pos, targetPos, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Invertir objetivo cuando llega
        if (Vector2.Distance(newPos, targetPos) <= 0.01f)
        {
            if (direction == MoveDirection.Horizontal)
            {
                float right = startPos.x + range;
                float left = startPos.x - range;
                targetPos = Mathf.Abs(targetPos.x - right) < 0.01f
                    ? new Vector2(left, startPos.y)
                    : new Vector2(right, startPos.y);
            }
            else
            {
                float up = startPos.y + range;
                float down = startPos.y - range;
                targetPos = Mathf.Abs(targetPos.y - up) < 0.01f
                    ? new Vector2(startPos.x, down)
                    : new Vector2(startPos.x, up);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Vector2 center = Application.isPlaying ? startPos : transform.position;

        Vector3 a, b;

        if (direction == MoveDirection.Horizontal)
        {
            a = new Vector3(center.x - range, center.y, 0f);
            b = new Vector3(center.x + range, center.y, 0f);
        }
        else
        {
            a = new Vector3(center.x, center.y - range, 0f);
            b = new Vector3(center.x, center.y + range, 0f);
        }

        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, 0.05f);
        Gizmos.DrawSphere(b, 0.05f);
    }
}
