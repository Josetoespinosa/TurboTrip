using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider2D))]
public class FinishLineTrigger : MonoBehaviour
{
    [Header("Scene to Load")]
    public string completionSceneName = "LoadingScene";

    [Header("Visual Feedback (Optional)")]
    public ParticleSystem finishEffect;
    public AudioClip finishSound;

    private bool hasTriggered = false;

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!IsPlayer(other)) return;

        hasTriggered = true;

        if (finishEffect) finishEffect.Play();
        if (finishSound) AudioSource.PlayClipAtPoint(finishSound, transform.position);

        if (LevelTimer.Instance != null)
            LevelTimer.Instance.FinishLevel();

        if (!string.IsNullOrEmpty(completionSceneName))
            SceneManager.LoadScene(completionSceneName);
        else
            Debug.LogWarning("Completion scene name not set on FinishLineTrigger!");
    }

    private static bool IsPlayer(Component c)
    {
        if (c.CompareTag("Player")) return true;
        // fallback por componentes del player
        var go = c.gameObject;
        return go.GetComponent<PlayerInputSimple>() != null
            || go.GetComponent<AbilitySystem>() != null
            || go.GetComponentInParent<PlayerInputSimple>() != null
            || go.GetComponentInParent<AbilitySystem>() != null;
    }
}
