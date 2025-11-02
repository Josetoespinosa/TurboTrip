using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Manages the world selection screen UI.
/// Dynamically creates buttons for each world and handles navigation to level selection.
/// </summary>
public class WorldSelectionUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Parent transform where world buttons will be spawned")]
    public Transform worldButtonContainer;
    [Tooltip("Prefab for world selection button (should have WorldButton component)")]
    public GameObject worldButtonPrefab;
    [Tooltip("Optional: Title text to display")]
    public TextMeshProUGUI titleText;
    
    [Header("Navigation")]
    [Tooltip("Name of the level selection scene")]
    public string levelSelectionSceneName = "LevelSelection";
    [Tooltip("Name of the home/main menu scene")]
    public string homeSceneName = "Home";

    public Button backButton;
    
    private GameProgressManager progressManager;
    
    void Start()
    {
        progressManager = GameProgressManager.Instance;

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButton);
        }
        
        if (progressManager == null)
        {
            Debug.LogError("GameProgressManager not found! Make sure it's in the scene or persists from a previous scene.");
            return;
        }
        
        Debug.Log($"GameProgressManager found with {progressManager.allWorlds?.Length ?? 0} worlds");
        
        /* if (titleText != null)
        {
            titleText.text = "SELECT WORLD";
        } */
        
        GenerateWorldButtons();
    }
    
    void GenerateWorldButtons()
    {
        if (worldButtonContainer == null)
        {
            Debug.LogError("World button container not assigned!");
            return;
        }
        
        if (worldButtonPrefab == null)
        {
            Debug.LogError("World button prefab not assigned!");
            return;
        }
        
        // Ensure container has a layout component to arrange buttons properly
        if (worldButtonContainer.GetComponent<VerticalLayoutGroup>() == null && 
            worldButtonContainer.GetComponent<HorizontalLayoutGroup>() == null &&
            worldButtonContainer.GetComponent<GridLayoutGroup>() == null)
        {
            var layoutGroup = worldButtonContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            Debug.Log("Added VerticalLayoutGroup to button container");
        }
        
        Debug.Log($"Generating buttons in container: {worldButtonContainer.name}");
        
        // Clear existing buttons
        foreach (Transform child in worldButtonContainer)
        {
            Destroy(child.gameObject);
        }
        
        if (progressManager.allWorlds == null || progressManager.allWorlds.Length == 0)
        {
            Debug.LogError("GameProgressManager has no worlds assigned! Assign WorldData assets to the allWorlds array.");
            return;
        }
        
        // Create button for each world
        int buttonCount = 0;
        foreach (var world in progressManager.allWorlds)
        {
            if (world == null)
            {
                Debug.LogWarning("Null world found in allWorlds array, skipping...");
                continue;
            }
            
            Debug.Log($"Creating button for world: {world.worldName}");
            GameObject buttonObj = Instantiate(worldButtonPrefab, worldButtonContainer);
            buttonObj.name = $"WorldButton_{world.worldName}";
            WorldButton worldButton = buttonObj.GetComponent<WorldButton>();
            
            if (worldButton != null)
            {
                bool isUnlocked = progressManager.IsWorldUnlocked(world);
                Debug.Log($"World {world.worldName} is {(isUnlocked ? "UNLOCKED" : "LOCKED")}");
                worldButton.Setup(world, isUnlocked, () => OnWorldSelected(world));
                buttonCount++;
            }
            else
            {
                Debug.LogError("WorldButton component not found on prefab!");
            }
        }
        
        Debug.Log($"Generated {buttonCount} world buttons");
    }
    
    void OnWorldSelected(WorldData world)
    {
        Debug.Log($"World button clicked: {world.worldName}");
        
        if (!progressManager.IsWorldUnlocked(world))
        {
            Debug.Log("World is locked!");
            // TODO: Show locked message or play sound
            return;
        }
        
        progressManager.SelectWorld(world);
        Debug.Log($"Loading level selection scene: {levelSelectionSceneName}");
        
        // Load level selection scene
        if (!string.IsNullOrEmpty(levelSelectionSceneName))
        {
            SceneManager.LoadScene(levelSelectionSceneName);
        }
        else
        {
            Debug.LogError("Level selection scene name not set!");
        }
    }
    
    public void OnBackButton()
    {
        Debug.Log("Returning to Home Menu");
        if (!string.IsNullOrEmpty(homeSceneName))
        {
            // UnityEngine.SceneManagement.SceneManager.LoadScene(homeSceneName);
            SceneManager.LoadScene(homeSceneName);
        }
        else
        {
            Debug.LogError("Home scene name not set!");
        }
    }
}
