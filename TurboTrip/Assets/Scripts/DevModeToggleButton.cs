using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Toggles dev mode to unlock all levels or restore normal progression.
/// Attach this to a button in the Home scene.
/// </summary>
public class DevModeToggleButton : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Optional: Text component to show current mode")]
    public TMP_Text buttonText;
    
    [Header("Button Text")]
    public string normalModeText = "Unlock All Levels";
    public string devModeText = "Restore Progression";
    
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(ToggleDevMode);
        }
        
        UpdateButtonText();
    }
    
    void ToggleDevMode()
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.ToggleDevMode();
            UpdateButtonText();
            
            if (GameProgressManager.Instance.IsDevModeActive())
            {
                Debug.Log("🎮 DEV MODE: All levels are now unlocked!");
            }
            else
            {
                Debug.Log("📋 NORMAL MODE: Progression system restored");
            }
        }
        else
        {
            Debug.LogError("GameProgressManager not found!");
        }
    }
    
    void UpdateButtonText()
    {
        if (buttonText == null) return;
        
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.IsDevModeActive())
        {
            buttonText.text = devModeText;
        }
        else
        {
            buttonText.text = normalModeText;
        }
    }
}
