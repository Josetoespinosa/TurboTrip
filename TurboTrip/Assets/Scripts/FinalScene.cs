using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    public TMP_Text timeText;
    public Button continueButton;
    public Button retryButton;
    public Button menuButton;

    void Start()
    {

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinue);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetry);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenu);
        }

        // Save level completion
        if (LevelTimer.Instance != null && GameProgressManager.Instance != null)
        {
            float elapsedTime = LevelTimer.Instance.GetElapsedTime();
            var currentLevel = GameProgressManager.Instance.selectedLevel;
            
            if (currentLevel != null)
            {
                GameProgressManager.Instance.CompleteLevel(currentLevel, elapsedTime);
                timeText.text = $"¡{currentLevel.levelName.ToUpper()} COMPLETADO! \n \n Tiempo total: {FormatTime(elapsedTime)}";
            }
            else
            {
                timeText.text = $"¡NIVEL COMPLETADO! \n \n Tiempo total: {FormatTime(elapsedTime)}";
            }
        }
        else
        {
            timeText.text = "Tiempo no disponible";
        }
        
        // Setup buttons
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinue);
        if (retryButton != null)
            retryButton.onClick.AddListener(OnRetry);
        if (menuButton != null)
            menuButton.onClick.AddListener(OnMenu);
    }

    void OnContinue()
    {
        Debug.Log("Continue button clicked - Loading LevelSelection");
        // Return to level selection to choose next level
        UnityEngine.SceneManagement.SceneManager.LoadScene("LevelSelection");
    }

    void OnRetry()
    {
        Debug.Log("Retry button clicked");
        // Reload current level
        var currentLevel = GameProgressManager.Instance?.selectedLevel;
        if (currentLevel != null && !string.IsNullOrEmpty(currentLevel.sceneName))
        {
            Debug.Log($"Retrying level: {currentLevel.sceneName}");
            // Reset timer
            if (LevelTimer.Instance != null)
            {
                Destroy(LevelTimer.Instance.gameObject);
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentLevel.sceneName);
        }
        else
        {
            Debug.LogError("Cannot retry - no level selected or scene name is empty!");
        }
    }

    void OnMenu()
    {
        Debug.Log("Menu button clicked - Loading Home");
        // Return to home menu
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        int milliseconds = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        
        if (minutes > 0)
            return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
        else
            return $"{seconds:00}.{milliseconds:00}";
    }
}