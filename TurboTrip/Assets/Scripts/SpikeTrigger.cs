using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    private PlayerRespawnManager respawnManager;

    void Start()
    {
        respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        bool isPlayer = collision.CompareTag("Player") || collision.GetComponentInParent<Nick>() != null;
        if (!isPlayer) return;
        if (respawnManager == null)
        {
            respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
            if (respawnManager == null) return;
        }
        respawnManager.KillPlayer();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        bool isPlayer = collision.collider.CompareTag("Player") || collision.collider.GetComponentInParent<Nick>() != null;
        if (!isPlayer) return;
        if (respawnManager == null)
        {
            respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
            if (respawnManager == null) return;
        }
        respawnManager.KillPlayer();
    }
}
