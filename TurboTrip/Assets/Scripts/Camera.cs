using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject player;
    public SpriteRenderer background; // Asigna el fondo aquí

    float minX, maxX, minY, maxY;

    void Start()
    {
        if (background == null) return;

        // Tamaño real del fondo en el mundo
        Bounds bgBounds = background.bounds;

        // Tamaño de la cámara (ortográfica)
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        // Calcular límites
        minX = bgBounds.min.x + camWidth / 2f;
        maxX = bgBounds.max.x - camWidth / 2f;
        minY = bgBounds.min.y + camHeight / 2f;
        maxY = bgBounds.max.y - camHeight / 2f;
    }

    void Update()
    {
        if (player == null) return;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(player.transform.position.x, minX, maxX);
        pos.y = Mathf.Clamp(player.transform.position.y, minY, maxY);

        transform.position = pos;
    }
}
