using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public Transform player;   // ← agrega referencia al jugador
    public float duration = 0.15f;
    public float shakePercentOfPlayer = 0.05f;
    // 0.05 = 5% del tamaño del jugador

    private Vector3 originalPos;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void ShakeOnce(float customDuration = -1f, float customPercent = -1f)
    {
        float d = customDuration > 0 ? customDuration : duration;
        float p = customPercent > 0 ? customPercent : shakePercentOfPlayer;

        StopAllCoroutines();
        StartCoroutine(DoShake(d, p));
    }

    IEnumerator DoShake(float dur, float percent)
    {
        float elapsed = 0f;

        // tamaño del jugador según su bounding box
        float playerSize = player ? player.localScale.magnitude : 1f;

        // magnitud real de la vibración
        float magnitude = playerSize * percent;

        while (elapsed < dur)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
