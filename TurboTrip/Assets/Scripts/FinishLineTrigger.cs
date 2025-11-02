using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple finish line trigger for levels.
/// Place this on a trigger collider at the end of your level.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FinishLineTrigger : MonoBehaviour
{
    [Header("Scene to Load")]
    [Tooltip("Scene to load when player finishes (usually LoadingScene)")]
    public string completionSceneName = "LoadingScene";
    
    [Header("Visual Feedback (Optional)")]
    public ParticleSystem finishEffect;
    public AudioClip finishSound;
    
    private bool hasTriggered = false;
    
    void Awake()
    {
        // Ensure this is a trigger
        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        
        // Check if it's the player
        bool isPlayer = other.CompareTag("Player") || other.GetComponentInParent<Nick>() != null;
        if (!isPlayer) return;
        
        hasTriggered = true;
        
        // Play visual effect
        if (finishEffect != null)
        {
            finishEffect.Play();
        }
        
        // Play sound
        if (finishSound != null)
        {
            AudioSource.PlayClipAtPoint(finishSound, transform.position);
        }
        
        // Stop level timer
        if (LevelTimer.Instance != null)
        {
            LevelTimer.Instance.FinishLevel();
        }
        
        // Load completion scene
        if (!string.IsNullOrEmpty(completionSceneName))
        {
            SceneManager.LoadScene(completionSceneName);
        }
        else
        {
            Debug.LogWarning("Completion scene name not set on FinishLineTrigger!");
        }
    }
}
