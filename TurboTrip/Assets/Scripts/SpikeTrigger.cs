using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    [SerializeField] private PlayerRespawnManager respawnManager;

    void Awake()
    {
        // autoconfig: si no lo asignaste a mano, lo busca en la escena
        if (!respawnManager)
#if UNITY_2023_1_OR_NEWER
            respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
#else
            respawnManager = FindObjectOfType<PlayerRespawnManager>();
#endif
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsPlayer(collision)) return;
        EnsureRespawnRef();
        respawnManager?.KillPlayer();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsPlayer(collision.collider)) return;
        EnsureRespawnRef();
        respawnManager?.KillPlayer();
    }

    private void EnsureRespawnRef()
    {
        if (respawnManager) return;
#if UNITY_2023_1_OR_NEWER
        respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
#else
        respawnManager = FindObjectOfType<PlayerRespawnManager>();
#endif
    }

    private static bool IsPlayer(Component c)
    {
        if (c.CompareTag("Player")) return true;
        var go = c.gameObject;
        return go.GetComponent<PlayerInputSimple>() != null
            || go.GetComponent<AbilitySystem>() != null
            || go.GetComponentInParent<PlayerInputSimple>() != null
            || go.GetComponentInParent<AbilitySystem>() != null;
    }
}
