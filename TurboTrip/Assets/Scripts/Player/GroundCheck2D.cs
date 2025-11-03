using UnityEngine;

[DisallowMultipleComponent]
public class GroundCheck2D : MonoBehaviour
{
    [Header("Raycast suelo")]
    public float groundRayDistance = 0.2f;
    public LayerMask groundMask = ~0;

    [Header("Lectura")]
    public bool grounded;

    void Update()
    {
        Debug.DrawRay(transform.position, Vector2.down * groundRayDistance, Color.red);
        grounded = Physics2D.Raycast(transform.position, Vector2.down, groundRayDistance, groundMask);
    }
}
