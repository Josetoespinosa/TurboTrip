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
    
    public void Setup(WorldData data, bool isUnlocked, Action onClick)
    {
        worldData = data;
        onClickCallback = onClick;
        
        Debug.Log($"Setting up WorldButton for {data.worldName}, unlocked: {isUnlocked}");
        
        // Set world info
        if (worldNameText != null)
            worldNameText.text = data.worldName;
        else
            Debug.LogWarning($"worldNameText is null on {gameObject.name}");
        
        if (worldDescriptionText != null)
            worldDescriptionText.text = data.description;
        
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
