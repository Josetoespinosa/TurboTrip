using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player progress including unlocked worlds, levels, and completion status.
/// Singleton pattern with PlayerPrefs persistence.
/// </summary>
public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }
    
    [Header("Game Data")]
    [Tooltip("All worlds in the game, in order")]
    public WorldData[] allWorlds;
    
    private HashSet<string> unlockedWorlds = new HashSet<string>();
    private HashSet<string> unlockedLevels = new HashSet<string>();
    private Dictionary<string, float> levelBestTimes = new Dictionary<string, float>();
    private Dictionary<string, bool> levelCompleted = new Dictionary<string, bool>();
    
    // Currently selected world/level for navigation
    public WorldData selectedWorld { get; private set; }
    public LevelData selectedLevel { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        LoadProgress();
        InitializeFirstTimeProgress();
    }
    
    #region Progress Management
    
    public bool IsWorldUnlocked(WorldData world)
    {
        if (world == null) return false;
        // Ignore unlockedByDefault flag - enforce progression system
        return unlockedWorlds.Contains(world.worldName);
    }
    
    public bool IsLevelUnlocked(LevelData level)
    {
        if (level == null) return false;
        // Ignore unlockedByDefault flag - enforce progression system
        return unlockedLevels.Contains(GetLevelKey(level));
    }
    
    public void UnlockWorld(WorldData world)
    {
        if (world == null) return;
        unlockedWorlds.Add(world.worldName);
        SaveProgress();
    }
    
    public void UnlockLevel(LevelData level)
    {
        if (level == null) return;
        unlockedLevels.Add(GetLevelKey(level));
        SaveProgress();
    }
    
    public void CompleteLevel(LevelData level, float time)
    {
        if (level == null) return;
        
        string key = GetLevelKey(level);
        levelCompleted[key] = true;
        
        // Update best time if this is better
        if (!levelBestTimes.ContainsKey(key) || time < levelBestTimes[key])
        {
            levelBestTimes[key] = time;
        }
        
        // Auto-unlock next level in the world
        UnlockNextLevel(level);
        
        SaveProgress();
    }
    
    public bool IsLevelCompleted(LevelData level)
    {
        if (level == null) return false;
        return levelCompleted.ContainsKey(GetLevelKey(level)) && levelCompleted[GetLevelKey(level)];
    }
    
    public float GetBestTime(LevelData level)
    {
        if (level == null) return 0f;
        string key = GetLevelKey(level);
        return levelBestTimes.ContainsKey(key) ? levelBestTimes[key] : 0f;
    }
    
    private void UnlockNextLevel(LevelData completedLevel)
    {
        // Find the world containing this level
        foreach (var world in allWorlds)
        {
            if (world == null || world.levels == null) continue;
            
            for (int i = 0; i < world.levels.Length; i++)
            {
                if (world.levels[i] == completedLevel)
                {
                    // If there's a next level in this world, unlock it
                    if (i + 1 < world.levels.Length)
                    {
                        UnlockLevel(world.levels[i + 1]);
                        Debug.Log($"Unlocked next level: {world.levels[i + 1].levelName}");
                    }
                    else
                    {
                        // This was the last level in the world, unlock next world
                        int worldIndex = System.Array.IndexOf(allWorlds, world);
                        if (worldIndex >= 0 && worldIndex + 1 < allWorlds.Length)
                        {
                            UnlockWorld(allWorlds[worldIndex + 1]);
                            Debug.Log($"Unlocked next world: {allWorlds[worldIndex + 1].worldName}");
                            
                            // Also unlock the first level of the next world
                            if (allWorlds[worldIndex + 1].levels != null && allWorlds[worldIndex + 1].levels.Length > 0)
                            {
                                UnlockLevel(allWorlds[worldIndex + 1].levels[0]);
                                Debug.Log($"Unlocked first level of next world: {allWorlds[worldIndex + 1].levels[0].levelName}");
                            }
                        }
                    }
                    return;
                }
            }
        }
    }
    
    #endregion
    
    #region Navigation
    
    public void SelectWorld(WorldData world)
    {
        selectedWorld = world;
        UnlockAbilitiesForWorld(world);
    }
    
    public void SelectLevel(LevelData level)
    {
        selectedLevel = level;
    }
    
    #endregion
    
    #region Ability Unlocking
    
    /// <summary>
    /// Sets abilities based on the world number (called when selecting a world)
    /// World 1: Base (no abilities)
    /// World 2: Double Jump
    /// World 3: Dash
    /// World 4: Wall Pass
    /// </summary>
    private void UnlockAbilitiesForWorld(WorldData world)
    {
        if (world == null) return;
        
        // Store which world's abilities should be active
        PlayerPrefs.SetInt("CurrentWorldNumber", world.worldNumber);
        PlayerPrefs.Save();
        
        Debug.Log($"Selected World {world.worldNumber} - abilities will be set when level loads");
    }
    
    /// <summary>
    /// Sets abilities for the current world when a level loads
    /// Call this when the player spawns in a level
    /// </summary>
    public void SetAbilitiesForCurrentWorld()
    {
        int worldNumber = PlayerPrefs.GetInt("CurrentWorldNumber", 1);
        
        // Find the player's AbilitySystem
        AbilitySystem abilitySystem = FindFirstObjectByType<AbilitySystem>();
        if (abilitySystem == null)
        {
            Debug.LogWarning("GameProgressManager: No AbilitySystem found in scene");
            return;
        }
        
        // First, reset all abilities
        abilitySystem.ResetAll();
        
        // Then unlock based on world number
        switch (worldNumber)
        {
            case 1:
                // World 1: Base - no abilities
                Debug.Log("World 1: Base abilities only (no special abilities)");
                break;
                
            case 2:
                // World 2: Unlock Double Jump
                abilitySystem.Unlock(AbilitySystem.Ability.DoubleJump);
                Debug.Log("World 2: Double Jump unlocked!");
                break;
                
            case 3:
                // World 3: Unlock Double Jump + Dash
                abilitySystem.Unlock(AbilitySystem.Ability.DoubleJump);
                abilitySystem.Unlock(AbilitySystem.Ability.Dash);
                Debug.Log("World 3: Double Jump + Dash unlocked!");
                break;
                
            case 4:
                // World 4: Unlock all abilities
                abilitySystem.Unlock(AbilitySystem.Ability.DoubleJump);
                abilitySystem.Unlock(AbilitySystem.Ability.Dash);
                abilitySystem.Unlock(AbilitySystem.Ability.WallPass);
                Debug.Log("World 4: All abilities unlocked!");
                break;
        }
    }
    
    #endregion
    
    #region Persistence
    
    private string GetLevelKey(LevelData level)
    {
        return $"{level.name}";
    }
    
    private void SaveProgress()
    {
        // Save unlocked worlds
        PlayerPrefs.SetString("UnlockedWorlds", string.Join(",", unlockedWorlds));
        
        // Save unlocked levels
        PlayerPrefs.SetString("UnlockedLevels", string.Join(",", unlockedLevels));
        
        // Save completed levels
        List<string> completed = new List<string>();
        foreach (var kvp in levelCompleted)
        {
            if (kvp.Value) completed.Add(kvp.Key);
        }
        PlayerPrefs.SetString("CompletedLevels", string.Join(",", completed));
        
        // Save best times
        foreach (var kvp in levelBestTimes)
        {
            PlayerPrefs.SetFloat($"BestTime_{kvp.Key}", kvp.Value);
        }
        
        PlayerPrefs.Save();
    }
    
    private void LoadProgress()
    {
        // Load unlocked worlds
        string worldsStr = PlayerPrefs.GetString("UnlockedWorlds", "");
        if (!string.IsNullOrEmpty(worldsStr))
        {
            unlockedWorlds = new HashSet<string>(worldsStr.Split(','));
        }
        
        // Load unlocked levels
        string levelsStr = PlayerPrefs.GetString("UnlockedLevels", "");
        if (!string.IsNullOrEmpty(levelsStr))
        {
            unlockedLevels = new HashSet<string>(levelsStr.Split(','));
        }
        
        // Load completed levels
        string completedStr = PlayerPrefs.GetString("CompletedLevels", "");
        if (!string.IsNullOrEmpty(completedStr))
        {
            foreach (string levelKey in completedStr.Split(','))
            {
                if (!string.IsNullOrEmpty(levelKey))
                {
                    levelCompleted[levelKey] = true;
                }
            }
        }
        
        // Load best times
        foreach (var world in allWorlds)
        {
            if (world == null || world.levels == null) continue;
            foreach (var level in world.levels)
            {
                if (level == null) continue;
                string key = GetLevelKey(level);
                if (PlayerPrefs.HasKey($"BestTime_{key}"))
                {
                    levelBestTimes[key] = PlayerPrefs.GetFloat($"BestTime_{key}");
                }
            }
        }
    }
    
    /// <summary>
    /// Initializes progress for first-time players
    /// Only unlocks World 1 and its first level
    /// </summary>
    private void InitializeFirstTimeProgress()
    {
        // Check if this is the first time running the game
        if (!PlayerPrefs.HasKey("GameInitialized"))
        {
            Debug.Log("First time running - initializing with World 1, Level 1 only");
            
            // Clear any existing progress
            unlockedWorlds.Clear();
            unlockedLevels.Clear();
            levelCompleted.Clear();
            levelBestTimes.Clear();
            
            // Unlock World 1
            if (allWorlds != null && allWorlds.Length > 0 && allWorlds[0] != null)
            {
                UnlockWorld(allWorlds[0]);
                Debug.Log($"Unlocked {allWorlds[0].worldName}");
                
                // Unlock only the first level of World 1
                if (allWorlds[0].levels != null && allWorlds[0].levels.Length > 0 && allWorlds[0].levels[0] != null)
                {
                    UnlockLevel(allWorlds[0].levels[0]);
                    Debug.Log($"Unlocked {allWorlds[0].levels[0].levelName}");
                }
            }
            
            // Mark as initialized
            PlayerPrefs.SetInt("GameInitialized", 1);
            SaveProgress();
        }
    }
    
    public void ResetProgress()
    {
        Debug.Log("Resetting all progress...");
        unlockedWorlds.Clear();
        unlockedLevels.Clear();
        levelBestTimes.Clear();
        levelCompleted.Clear();
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // Reinitialize with only World 1, Level 1
        InitializeFirstTimeProgress();
        Debug.Log("Progress reset complete - only World 1, Level 1 is unlocked");
    }
    
    #endregion
}
