using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Component for individual level selection button.
/// Attach this to your level button prefab.
/// </summary>
[RequireComponent(typeof(Button))]
public class LevelButton : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI levelNumberText;
    public TextMeshProUGUI bestTimeText;
    public Image levelIconImage;
    public Image backgroundImage;
    public GameObject lockedOverlay;
    public GameObject completedIndicator;
    public Image[] stars; // Optional: for star rating system
    
    private Button button;
    private LevelData levelData;
    private Action onClickCallback;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    public void Setup(LevelData data, bool isUnlocked, bool isCompleted, float bestTime, Action onClick)
    {
        levelData = data;
        onClickCallback = onClick;
        
        // Set level info
        if (levelNameText != null)
            levelNameText.text = data.levelName;
        else
        {
            // Fallback: try to find button's text component
            var buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
                buttonText.text = data.levelName;
        }
        
        if (levelNumberText != null)
            levelNumberText.text = $"Level {data.levelNumber}";
        
        if (levelIconImage != null && data.levelIcon != null)
        {
            levelIconImage.sprite = data.levelIcon;
            levelIconImage.enabled = true;
        }
        else if (levelIconImage != null)
        {
            levelIconImage.enabled = false;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = data.levelColor;
        }
        
        // Set best time
        if (bestTimeText != null)
        {
            if (bestTime > 0f)
            {
                bestTimeText.text = $"Best: {FormatTime(bestTime)}";
                bestTimeText.gameObject.SetActive(true);
            }
            else
            {
                bestTimeText.gameObject.SetActive(false);
            }
        }
        
        // Set completion indicator
        if (completedIndicator != null)
        {
            completedIndicator.SetActive(isCompleted);
        }
        
        // Set locked state
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }
        
        // Configure button
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke());
        button.interactable = isUnlocked;
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
