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
    
    public void Setup(LevelData data, bool isUnlocked, bool isCompleted, float bestTime, Action onClick, Sprite buttonSprite = null, float width = 300f, float height = 100f)
    {
        levelData = data;
        onClickCallback = onClick;
        
        // Set button dimensions
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = new Vector2(width, height);
        }
        
        // Add LayoutElement to enforce custom size (prevents layout group from overriding)
        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = gameObject.AddComponent<LayoutElement>();
        }
        layoutElement.preferredWidth = width;
        layoutElement.preferredHeight = height;
        
        // Set button image
        if (buttonSprite != null)
        {
            Image buttonImage = GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = buttonSprite;
            }
        }
        
        // Set level info
        if (levelNameText != null)
        {
            levelNameText.text = data.levelName;
            levelNameText.color = Color.white;
        }
        else
        {
            // Fallback: try to find button's text component
            var buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = data.levelName;
                buttonText.color = Color.white;
            }
        }
        
        if (levelNumberText != null)
        {
            levelNumberText.text = $"Level {data.levelNumber}";
            levelNumberText.color = Color.white;
        }
        
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
                bestTimeText.color = Color.white;
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
