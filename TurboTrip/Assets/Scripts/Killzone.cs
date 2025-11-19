using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private PlayerRespawnManager respawnManager;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            EnsureRespawnRef();
            respawnManager?.KillPlayer();
        }
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
}
