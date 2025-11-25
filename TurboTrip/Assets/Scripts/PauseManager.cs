using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the pause menu functionality for all levels.
/// Allows pausing the game, resuming, restarting, and exiting to menu.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main pause menu panel that will be shown/hidden")]
    public GameObject pauseMenuPanel;
    
    [Header("Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button exitButton;
    
    [Header("Settings")]
    [Tooltip("Scene to load when exiting (usually 'Home' or 'LevelSelection')")]
    public string menuSceneName = "LevelSelection";
    
    private bool isPaused = false;
    private bool isInitialized = false;
    private float initializeDelay = 0.1f; // Small delay to prevent input on first frame
    
    void Awake()
    {
        // Force reset pause state and time scale immediately on load
        isPaused = false;
        Time.timeScale = 1f;
        isInitialized = false;
        
        // Fix Canvas position if needed
        if (pauseMenuPanel != null)
        {
            Canvas canvas = pauseMenuPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    // Fix Z position to 0 (was -8 in some worlds)
                    Vector3 pos = canvasRect.localPosition;
                    pos.z = 0f;
                    canvasRect.localPosition = pos;
                    Debug.Log($"Canvas Z position fixed from {pos.z} to 0");
                }
            }
        }
        
        // Make sure pause menu is hidden at start
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }
    
    void Start()
    {
        // Check for EventSystem (required for UI button clicks)
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            Debug.LogError("No EventSystem found! UI buttons won't work. Creating one...");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
        
        // Double-check pause menu is hidden
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        // Setup button listeners (only add if not already added)
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(Resume);
            Debug.Log("Resume button listener added");
        }
        else
        {
            Debug.LogWarning("Resume button is not assigned!");
        }
        
        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartLevel);
            Debug.Log("Restart button listener added");
        }
        else
        {
            Debug.LogWarning("Restart button is not assigned!");
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitToMenu);
            Debug.Log("Exit button listener added");
        }
        else
        {
            Debug.LogWarning("Exit button is not assigned!");
        }
        
        // Mark as initialized after a short delay
        Invoke(nameof(MarkInitialized), initializeDelay);
    }
    
    void MarkInitialized()
    {
        isInitialized = true;
    }
    
    void Update()
    {
        // Don't accept input until initialized
        if (!isInitialized)
            return;
            
        // Check for pause input (ESC key or Pause key) using new Input System
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
            {
                if (isPaused)
                {
                    Resume();
                }
                else
                {
                    Pause();
                }
            }
        }
    }
    
    /// <summary>
    /// Pauses the game and shows the pause menu
    /// </summary>
    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freeze game time
        
        // Disable player input
        DisablePlayerInput();
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
            Debug.Log("Game Paused - Menu shown");
        }
        else
        {
            Debug.LogError("PauseManager: pauseMenuPanel is NULL! Cannot show pause menu. Please assign it in the Inspector.");
        }
    }
    
    /// <summary>
    /// Resumes the game and hides the pause menu
    /// </summary>
    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resume game time
        
        // Re-enable player input
        EnablePlayerInput();
        
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        
        Debug.Log("Game Resumed");
    }
    
    /// <summary>
    /// Disable player input components when paused
    /// </summary>
    private void DisablePlayerInput()
    {
        PlayerInputSimple playerInput = FindFirstObjectByType<PlayerInputSimple>();
        if (playerInput != null)
        {
            playerInput.enabled = false;
        }
    }
    
    /// <summary>
    /// Re-enable player input components when resumed
    /// </summary>
    private void EnablePlayerInput()
    {
        PlayerInputSimple playerInput = FindFirstObjectByType<PlayerInputSimple>();
        if (playerInput != null)
        {
            playerInput.enabled = true;
        }
    }
    
    /// <summary>
    /// Restarts the current level
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log("Restarting Level");
        
        // Reset time scale before loading
        Time.timeScale = 1f;
        isPaused = false;
        
        // Destroy the timer instance if it exists so it can restart fresh
        if (LevelTimer.Instance != null)
        {
            Destroy(LevelTimer.Instance.gameObject);
        }
        
        // Reload the current scene
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
    
    /// <summary>
    /// Exits to the menu (Level Selection or Home)
    /// </summary>
    public void ExitToMenu()
    {
        Debug.Log($"Exiting to {menuSceneName}");
        
        // Reset time scale before loading
        Time.timeScale = 1f;
        isPaused = false;
        
        // Destroy the timer instance if it exists
        if (LevelTimer.Instance != null)
        {
            Destroy(LevelTimer.Instance.gameObject);
        }
        
        // Load the menu scene
        SceneManager.LoadScene(menuSceneName);
    }
    
    /// <summary>
    /// Check if the game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    void OnDestroy()
    {
        // Ensure time scale is reset when this object is destroyed
        if (isPaused)
        {
            Time.timeScale = 1f;
        }
    }
}
