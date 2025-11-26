using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class AbilitySystem : MonoBehaviour
{
    public enum Ability { DoubleJump, Dash, WallPass }

    [System.Serializable]
    public class AbilityUnlockEvent : UnityEvent<Ability> { }

    [Header("Estado de habilidades")]
    [SerializeField] private bool hasDoubleJump = false; // bloqueado al inicio
    [SerializeField] private bool hasDash = false;       // bloqueado al inicio
    [SerializeField] private bool hasWallPass = false;   // bloqueado al inicio

    [Header("Eventos")]
    public AbilityUnlockEvent OnAbilityUnlocked;

    // Claves de guardado (PlayerPrefs)
    private const string KEY_DOUBLE_JUMP = "Ability_DoubleJump";
    private const string KEY_DASH = "Ability_Dash";
    private const string KEY_WALL_PASS = "Ability_WallPass";

    private void Awake()
    {
        LoadState();
    }

    // ---- API pública ----
    public bool Has(Ability a) => a switch
    {
        Ability.DoubleJump => hasDoubleJump,
        Ability.Dash => hasDash,
        Ability.WallPass => hasWallPass,
        _ => false
    };

    public bool Unlock(Ability a)
    {
        if (Has(a)) return false;

        switch (a)
        {
            case Ability.DoubleJump: hasDoubleJump = true; break;
            case Ability.Dash: hasDash = true; break;
            case Ability.WallPass: hasWallPass = true; break;
        }

        SaveState(); // <<< guarda al desbloquear
        OnAbilityUnlocked?.Invoke(a);
        return true;
    }

    /// <summary>
    /// Resetea todas las habilidades (por ejemplo, al empezar una nueva partida).
    /// </summary>
    public void ResetAll()
    {
        hasDoubleJump = false;
        hasDash = false;
        hasWallPass = false;
        SaveState();
    }

    // ---- Persistencia ----
    private void LoadState()
    {
        // Si no existe la clave, devuelve 0 (false)
        hasDoubleJump = PlayerPrefs.GetInt(KEY_DOUBLE_JUMP, 0) == 1;
        hasDash = PlayerPrefs.GetInt(KEY_DASH, 0) == 1;
        hasWallPass = PlayerPrefs.GetInt(KEY_WALL_PASS, 0) == 1;
    }

    private void SaveState()
    {
        PlayerPrefs.SetInt(KEY_DOUBLE_JUMP, hasDoubleJump ? 1 : 0);
        PlayerPrefs.SetInt(KEY_DASH, hasDash ? 1 : 0);
        PlayerPrefs.SetInt(KEY_WALL_PASS, hasWallPass ? 1 : 0);
        PlayerPrefs.Save();
    }
}
