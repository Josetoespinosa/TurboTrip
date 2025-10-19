using UnityEngine;
using System.Collections;

public class PlayerRespawnManager : MonoBehaviour
{
    public Transform respawnPoint;
    public GameObject playerPrefab;
    private GameObject currentPlayer;
    [Tooltip("Vertical offset applied on respawn to avoid immediate ground/spike overlap")]
    public float respawnYOffset = 0.5f;

    void Start()
    {
        // If a player already exists in the scene, adopt it instead of spawning a new one
        var existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            currentPlayer = existingPlayer;
            if (respawnPoint == null)
            {
                // Default initial checkpoint to player's current position
                var rp = new GameObject("InitialRespawnPoint").transform;
                rp.position = existingPlayer.transform.position;
                respawnPoint = rp;
            }
            UpdateCameraTarget();
            return;
        }

        if (playerPrefab == null) return;
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        if (playerPrefab == null) return;
        Vector3 spawnPos = respawnPoint != null ? respawnPoint.position : transform.position;
        currentPlayer = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        // Focus camera on the player
        UpdateCameraTarget();
    }

    public void KillPlayer()
    {
        if (currentPlayer == null)
        {
            SpawnPlayer();
            return;
        }
        StartCoroutine(RespawnCoroutine());
    }

    // Call this from a checkpoint trigger to update the last checkpoint
    public void SetCheckpoint(Transform checkpoint)
    {
        if (checkpoint == null) return;
        respawnPoint = checkpoint;
    }

    private void UpdateCameraTarget()
    {
        var cam = FindAnyObjectByType<CameraScript>();
        if (cam != null)
        {
            cam.player = currentPlayer;
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        if (currentPlayer == null)
        {
            SpawnPlayer();
            yield break;
        }

        // Disable colliders briefly to avoid immediate re-trigger with spikes
        var colliders = currentPlayer.GetComponentsInChildren<Collider2D>(true);
        foreach (var c in colliders) c.enabled = false;

        // Reset physics and move to checkpoint
        var rb = currentPlayer.GetComponent<Rigidbody2D>();
        Vector3 spawnPos = (respawnPoint != null ? respawnPoint.position : transform.position) + new Vector3(0f, respawnYOffset, 0f);
        currentPlayer.transform.position = spawnPos;
        currentPlayer.transform.rotation = Quaternion.identity;

        if (rb != null)
        {
            // Ensure physics is active and reset motion
            rb.simulated = true;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            if (Mathf.Approximately(rb.gravityScale, 0f)) rb.gravityScale = 1f;
        }

        // Wait one physics step before re-enabling colliders
        yield return new WaitForFixedUpdate();
        foreach (var c in colliders) c.enabled = true;

        // Notify listeners if any
        currentPlayer.SendMessage("OnRespawn", SendMessageOptions.DontRequireReceiver);
    }
}
