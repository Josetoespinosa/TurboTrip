using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// Component for individual world selection button.
/// Attach this to your world button prefab.
/// </summary>
[RequireComponent(typeof(Button))]
public class WorldButton : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI worldNameText;
    public TextMeshProUGUI worldDescriptionText;
    public Image worldIconImage;
    public Image backgroundImage;
    public GameObject lockedOverlay;
    public TextMeshProUGUI lockedText;
    
    private Button button;
    private WorldData worldData;
    private Action onClickCallback;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    public void Setup(WorldData data, bool isUnlocked, Action onClick, Sprite buttonSprite = null, float width = 300f, float height = 100f)
    {
        worldData = data;
        onClickCallback = onClick;
        
        Debug.Log($"Setting up WorldButton for {data.worldName}, unlocked: {isUnlocked}");
        
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
        
        // Set world info
        if (worldNameText != null)
        {
            worldNameText.text = data.worldName;
            worldNameText.color = Color.white;
        }
        else
        {
            // Fallback: try to find button's text component
            var buttonText = GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = data.worldName;
                buttonText.color = Color.white;
            }
            else
                Debug.LogWarning($"worldNameText is null on {gameObject.name}");
        }
        
        if (worldDescriptionText != null)
        {
            worldDescriptionText.text = data.description;
            worldDescriptionText.color = Color.white;
        }
        
        if (worldIconImage != null && data.worldIcon != null)
        {
            worldIconImage.sprite = data.worldIcon;
            worldIconImage.enabled = true;
        }
        else if (worldIconImage != null)
        {
            worldIconImage.enabled = false;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = data.worldColor;
        }
        
        // Set locked state
        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!isUnlocked);
        }
        
        if (lockedText != null)
        {
            lockedText.gameObject.SetActive(!isUnlocked);
        }
        
        // Configure button
        if (button == null)
        {
            Debug.LogError($"Button component is null on {gameObject.name}!");
            return;
        }
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            Debug.Log($"WorldButton clicked: {data.worldName}");
            onClickCallback?.Invoke();
        });
        button.interactable = isUnlocked;
        
        Debug.Log($"WorldButton setup complete. Button interactable: {button.interactable}");
    }
}
