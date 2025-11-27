using UnityEngine;

/// <summary>
/// Initializes player abilities based on the current world when a level starts.
/// Add this to your Player prefab or create an empty GameObject in each level with this script.
/// </summary>
public class LevelAbilityInitializer : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Delay before setting abilities (in seconds)")]
    public float initDelay = 0.1f;
    
    void Start()
    {
        Invoke(nameof(InitializeAbilities), initDelay);
    }
    
    private void InitializeAbilities()
    {
        if (GameProgressManager.Instance != null)
        {
            GameProgressManager.Instance.SetAbilitiesForCurrentWorld();
            Debug.Log("LevelAbilityInitializer: Abilities set for current world");
        }
        else
        {
            Debug.LogWarning("LevelAbilityInitializer: GameProgressManager not found!");
        }
    }
}
