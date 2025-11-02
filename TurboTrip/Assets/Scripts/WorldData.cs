using UnityEngine;

[CreateAssetMenu(fileName = "NewWorld", menuName = "TurboTrip/World Data")]
public class WorldData : ScriptableObject
{
    [Header("World Info")]
    public string worldName = "World 1";
    [TextArea(2, 4)]
    public string description = "The beginning of your journey.";
    public int worldNumber = 1;
    
    [Header("Levels")]
    [Tooltip("All levels in this world")]
    public LevelData[] levels;
    
    [Header("Requirements")]
    [Tooltip("Is this world unlocked by default?")]
    public bool unlockedByDefault = true;
    
    [Header("Visual")]
    public Sprite worldIcon;
    public Color worldColor = Color.cyan;
}
