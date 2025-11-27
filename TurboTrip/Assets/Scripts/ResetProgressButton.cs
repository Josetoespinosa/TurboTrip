using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Resets all game progress when clicked.
/// Useful for testing. Attach to a button in the Home scene.
/// </summary>
public class ResetProgressButton : MonoBehaviour
{
    void Start()
    {
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ResetProgress);
        }
    }
    
    void ResetProgress()
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ResetProgress();
            Debug.Log("Progress has been reset! Only World 1 Level 1 is unlocked.");
        }
    }
}
