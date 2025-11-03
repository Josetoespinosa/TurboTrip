using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the level selection screen UI for a chosen world.
/// Dynamically creates buttons for each level and handles loading the level scene.
/// </summary>
public class LevelSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent transform where level buttons will be spawned")]
    public Transform levelButtonContainer;
    [Tooltip("Prefab for level selection button (should have LevelButton component)")]
    public GameObject levelButtonPrefab;
    [Tooltip("Title text showing current world name")]
    public TextMeshProUGUI worldTitleText;
    [Tooltip("Description text for the world")]
    public TextMeshProUGUI worldDescriptionText;

    [Header("Navigation")]
    [Tooltip("Name of the world selection scene to return to")]
    public string worldSelectionSceneName = "WorldSelection";
    
    public Button backButton;
    private GameProgressManager progressManager;
    private WorldData currentWorld;
    
    void Start()
    {
        progressManager = GameProgressManager.Instance;
        
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButton);
        }

        if (progressManager == null)
        {
            Debug.LogError("GameProgressManager not found!");
            return;
        }
        
        currentWorld = progressManager.selectedWorld;
        
        if (currentWorld == null)
        {
            Debug.LogError("No world selected! Returning to world selection.");
            OnBackButton();
            return;
        }
        
        SetupWorldInfo();
        GenerateLevelButtons();
    }
    
    void SetupWorldInfo()
    {
        if (worldTitleText != null)
        {
            worldTitleText.text = currentWorld.worldName;
        }
        else
        {
            // Try to find a TextMeshPro component named "Title" as fallback
            var titleObj = GameObject.Find("Title");
            if (titleObj != null)
            {
                var titleText = titleObj.GetComponent<TextMeshProUGUI>();
                if (titleText != null)
                {
                    titleText.text = currentWorld.worldName;
                }
            }
        }
        
        if (worldDescriptionText != null)
        {
            worldDescriptionText.text = currentWorld.description;
        }
    }
    
    void GenerateLevelButtons()
    {
        if (levelButtonContainer == null || levelButtonPrefab == null)
        {
            Debug.LogError("Level button container or prefab not assigned!");
            return;
        }
        
        // Ensure container has a layout component to arrange buttons properly (same as WorldSelectionUI)
        if (levelButtonContainer.GetComponent<VerticalLayoutGroup>() == null && 
            levelButtonContainer.GetComponent<HorizontalLayoutGroup>() == null &&
            levelButtonContainer.GetComponent<GridLayoutGroup>() == null)
        {
            var layoutGroup = levelButtonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            Debug.Log("Added VerticalLayoutGroup to level button container");
        }
        
        // Clear existing buttons
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create button for each level
        if (currentWorld.levels == null || currentWorld.levels.Length == 0)
        {
            Debug.LogWarning($"World {currentWorld.worldName} has no levels assigned!");
            return;
        }
        
        foreach (var level in currentWorld.levels)
        {
            if (level == null) continue;
            
            GameObject buttonObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            LevelButton levelButton = buttonObj.GetComponent<LevelButton>();
            
            if (levelButton != null)
            {
                bool isUnlocked = progressManager.IsLevelUnlocked(level);
                bool isCompleted = progressManager.IsLevelCompleted(level);
                float bestTime = progressManager.GetBestTime(level);
                
                levelButton.Setup(level, isUnlocked, isCompleted, bestTime, () => OnLevelSelected(level));
            }
            else
            {
                Debug.LogWarning("LevelButton component not found on prefab!");
            }
        }
    }
    
    void OnLevelSelected(LevelData level)
    {
        if (!progressManager.IsLevelUnlocked(level))
        {
            Debug.Log("Level is locked!");
            // TODO: Show locked message or play sound
            return;
        }
        
        progressManager.SelectLevel(level);
        
        // Load the level scene
        if (!string.IsNullOrEmpty(level.sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(level.sceneName);
        }
        else
        {
            Debug.LogError($"Scene name not set for level {level.levelName}!");
        }
    }
    
    public void OnBackButton()
    {
        if (!string.IsNullOrEmpty(worldSelectionSceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(worldSelectionSceneName);
        }
        else
        {
            Debug.LogError("World selection scene name not set!");
        }
    }
}
