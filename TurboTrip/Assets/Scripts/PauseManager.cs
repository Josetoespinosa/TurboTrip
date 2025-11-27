using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the pause menu functionality for all levels.
/// Allows pausing the game, resuming, restarting, and exiting to menu.
/// Includes cheats such as NOCLIP.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pauseMenuPanel;

    [Header("Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button exitButton;

    [Header("Settings")]
    public string menuSceneName = "LevelSelection";

    [Header("Cheats")]
    public bool noclipEnabled = false;

    private bool isPaused = false;
    private bool isInitialized = false;
    private float initializeDelay = 0.1f;

    void Awake()
    {
        // SOLO ocultar el panel cuando est� ejecut�ndose el juego
        if (Application.isPlaying)
        {
            isPaused = false;
            Time.timeScale = 1f;
            isInitialized = false;

            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);
        }
    }



    void Start()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        if (Application.isPlaying && pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(Resume);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartLevel);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(ExitToMenu);
        }

        Invoke(nameof(MarkInitialized), initializeDelay);

#if UNITY_EDITOR
        // En el editor NO desactives el panel
        if (!Application.isPlaying && pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
#endif
    }

    void MarkInitialized()
    {
        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized)
            return;

        var keyboard = Keyboard.current;

        // ---- Toggle Pause ----
        if (keyboard != null)
        {
            if (keyboard.escapeKey.wasPressedThisFrame || keyboard.pKey.wasPressedThisFrame)
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        // ---- Cheats only work while paused ----
        if (isPaused && keyboard != null)
        {
            if (keyboard.nKey.wasPressedThisFrame)
            {
                ToggleNoClip();
            }
        }

        if (noclipEnabled)
        {
            var player = FindFirstObjectByType<PlayerInputSimple>();
            if (player != null)
            {
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                var keyb = Keyboard.current;

                float moveSpeed = 12f; // velocidad del noclip
                Vector2 move = Vector2.zero;

                if (keyb.wKey.isPressed) move += Vector2.up;
                if (keyb.sKey.isPressed) move += Vector2.down;
                if (keyb.aKey.isPressed) move += Vector2.left;
                if (keyb.dKey.isPressed) move += Vector2.right;

                // Movimiento instant�neo sin f�sica
                rb.linearVelocity = move.normalized * moveSpeed;
            }
        }

    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;

        DisablePlayerInput();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;

        EnablePlayerInput();

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void DisablePlayerInput()
    {
        PlayerInputSimple playerInput = FindFirstObjectByType<PlayerInputSimple>();
        if (playerInput != null)
            playerInput.enabled = false;
    }

    private void EnablePlayerInput()
    {
        PlayerInputSimple playerInput = FindFirstObjectByType<PlayerInputSimple>();
        if (playerInput != null)
            playerInput.enabled = true;
    }

    public void RestartLevel()
    {
        Debug.Log("Restarting Level");
        Time.timeScale = 1f;
        isPaused = false;

        // Destroy timer instance completely
        if (LevelTimer.Instance != null)
        {
            GameObject timerObj = LevelTimer.Instance.gameObject;
            LevelTimer.Instance = null;
            Destroy(timerObj);
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Debug.Log("Exiting to menu");
        Time.timeScale = 1f;
        isPaused = false;

        // Destroy timer instance completely
        if (LevelTimer.Instance != null)
        {
            GameObject timerObj = LevelTimer.Instance.gameObject;
            LevelTimer.Instance = null;
            Destroy(timerObj);
        }

        SceneManager.LoadScene(menuSceneName);
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    void OnDestroy()
    {
        if (isPaused)
            Time.timeScale = 1f;
    }


    private void ToggleNoClip()
    {
        noclipEnabled = !noclipEnabled;

        var player = FindFirstObjectByType<PlayerInputSimple>();
        if (player == null)
        {
            Debug.LogWarning("NOCLIP: No player found.");
            return;
        }

        var collider = player.GetComponent<Collider2D>();
        var rb = player.GetComponent<Rigidbody2D>();
        var input = player.GetComponent<PlayerInputSimple>();

        // Desactivar controles normales durante noclip
        if (input != null)
            input.enabled = !noclipEnabled;

        if (collider != null)
            collider.enabled = !noclipEnabled;

        if (noclipEnabled)
        {
            rb.gravityScale = 0;
            rb.linearDamping = 8f;
            rb.angularDamping = 8f;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.gravityScale = 1;
            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
        }

        Debug.Log("NOCLIP: " + (noclipEnabled ? "ACTIVADO" : "DESACTIVADO"));
    }

}
