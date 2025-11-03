using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class AbilitySystem : MonoBehaviour
{
    public enum Ability { DoubleJump, Dash, WallPass }

    [System.Serializable] public class AbilityUnlockEvent : UnityEvent<Ability> { }

    [Header("Estado de habilidades")]
    [SerializeField] private bool hasDoubleJump = false; // bloqueado al inicio
    [SerializeField] private bool hasDash = false; // bloqueado al inicio
    [SerializeField] private bool hasWallPass = false; // bloqueado al inicio

    [Header("Eventos")]
    public AbilityUnlockEvent OnAbilityUnlocked;

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
        OnAbilityUnlocked?.Invoke(a);
        return true;
    }
}
