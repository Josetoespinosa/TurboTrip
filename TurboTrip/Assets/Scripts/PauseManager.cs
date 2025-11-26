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
        isPaused = false;
        Time.timeScale = 1f;
        isInitialized = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);

            Canvas canvas = pauseMenuPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    Vector3 pos = canvasRect.localPosition;
                    pos.z = 0f;
                    canvasRect.localPosition = pos;
                }
            }
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

        if (pauseMenuPanel != null)
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

                // Movimiento instantáneo sin física
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
        Time.timeScale = 1f;
        isPaused = false;

        if (LevelTimer.Instance != null)
            Destroy(LevelTimer.Instance.gameObject);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (LevelTimer.Instance != null)
            Destroy(LevelTimer.Instance.gameObject);

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
