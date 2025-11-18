using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HorizontalSawHazard : MonoBehaviour
{
    [Header("Movimiento horizontal")]
    [Tooltip("Distancia que se moverá hacia la derecha e izquierda desde la posición inicial.")]
    public float range = 3f;

    [Tooltip("Velocidad de movimiento de la sierra.")]
    public float speed = 3f;

    private Rigidbody2D rb;
    private Collider2D col;

    private float startX;
    private float targetX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Configuración de físicas para hazard tipo trigger
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Queremos detección sin colisión física → usar Trigger
        col.isTrigger = true;

        // Guardamos la posición inicial y definimos primer objetivo (a la derecha)
        startX = transform.position.x;
        targetX = startX + range;
    }

    private void FixedUpdate()
    {
        Vector2 pos = rb.position;
        float newX = Mathf.MoveTowards(pos.x, targetX, speed * Time.fixedDeltaTime);
        rb.MovePosition(new Vector2(newX, pos.y));

        // Cuando llega (o casi) al objetivo, invertimos la dirección
        if (Mathf.Abs(newX - targetX) <= 0.01f)
        {
            // Si estaba yendo a startX + range, ahora va a startX - range, y viceversa
            float rightX = startX + range;
            float leftX = startX - range;

            if (Mathf.Abs(targetX - rightX) <= 0.01f)
                targetX = leftX;
            else
                targetX = rightX;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        float x = Application.isPlaying ? startX : transform.position.x;
        float y = transform.position.y;

        Vector3 left = new Vector3(x - range, y, 0f);
        Vector3 right = new Vector3(x + range, y, 0f);

        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.05f);
        Gizmos.DrawSphere(right, 0.05f);
    }
}
