using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "TurboTrip/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Level Info")]
    public string levelName = "Level 1";
    [TextArea(2, 4)]
    public string description = "A challenging level to test your skills.";
    public int levelNumber = 1;
    
    [Header("Scene")]
    [Tooltip("The name of the scene to load for this level")]
    public string sceneName = "Level_1_1";
    
    [Header("Requirements")]
    [Tooltip("Is this level unlocked by default?")]
    public bool unlockedByDefault = false;
    
    [Header("Visual")]
    public Sprite levelIcon;
    public Color levelColor = Color.white;
    
    [Header("Objectives (Optional)")]
    public float targetTime = 60f;
    public int starsToUnlock = 0;
}
