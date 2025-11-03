using UnityEngine;
using System;
using System.Collections;

public class PlayerRespawnManager : MonoBehaviour
{
    public Transform respawnPoint;
    public GameObject playerPrefab;
    private GameObject currentPlayer;
    [Tooltip("Vertical offset applied on respawn to avoid immediate ground/spike overlap")]
    public float respawnYOffset = 0.5f;
    [Header("Death animation")]
    [Tooltip("Animator trigger name used to play the death animation on the player (optional)")]
    public string deathTriggerName = "";
    [Tooltip("Animator boolean parameter name used to indicate the player is dead; leave empty to disable")]
    public string deathBoolName = "isDead";
    [Tooltip("Optional name of the death animation clip. If set and found in the controller, its length will be used to wait before respawn")]
    public string deathClipName = "";
    [Tooltip("Fallback delay (seconds) to wait for the death animation before respawning when clip length can't be determined")]
    public float deathAnimationDelay = 0.1f;

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
        StartCoroutine(PlayDeathAndRespawn());
    }

    private IEnumerator PlayDeathAndRespawn()
    {
        if (currentPlayer == null)
        {
            SpawnPlayer();
            yield break;
        }

        // Notify other components about death
        currentPlayer.SendMessage("OnDeath", SendMessageOptions.DontRequireReceiver);

        // Try to play death animation on the player's animator
        Animator anim = currentPlayer.GetComponentInChildren<Animator>();
        float waitTime = Mathf.Max(0f, deathAnimationDelay);

        if (anim != null)
        {
            try
            {
                if (!string.IsNullOrEmpty(deathBoolName))
                    anim.SetBool(deathBoolName, true);
                else if (!string.IsNullOrEmpty(deathTriggerName))
                    anim.SetTrigger(deathTriggerName);

                var controller = anim.runtimeAnimatorController;
                if (controller != null && !string.IsNullOrEmpty(deathClipName))
                {
                    foreach (var clip in controller.animationClips)
                    {
                        if (clip == null) continue;
                        if (clip.name.Equals(deathClipName, StringComparison.OrdinalIgnoreCase))
                        {
                            waitTime = clip.length;
                            break;
                        }
                    }
                }
            }
            catch (Exception)
            {
                waitTime = Mathf.Max(0f, deathAnimationDelay);
            }
        }

        if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

        // After animation (or fallback) proceed to respawn
        yield return StartCoroutine(RespawnCoroutine());

        // Clear animator death bool if set
        if (anim != null && !string.IsNullOrEmpty(deathBoolName))
            anim.SetBool(deathBoolName, false);
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
