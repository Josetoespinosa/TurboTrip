using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple home/main menu controller.
/// Handles Play and Exit buttons.
/// </summary>
public class HomeManager : MonoBehaviour
{
    [Header("Buttons")]
    public Button playButton;
    public Button exitButton;
    
    [Header("Settings")]
    public string worldSelectionSceneName = "WorldSelection";
    
    void Start()
    {
        // Setup button listeners
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked);
        }
        
        // Ensure GameProgressManager exists
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("GameProgressManager not found in scene. Creating one...");
            GameObject managerObj = new GameObject("GameProgressManager");
            managerObj.AddComponent<GameProgressManager>();
        }
    }
    
    void OnPlayClicked()
    {
        Debug.Log("Play button clicked - Loading World Selection");
        SceneManager.LoadScene(worldSelectionSceneName);
    }
    
    void OnExitClicked()
    {
        Debug.Log("Exit button clicked - Quitting game");
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
