using UnityEngine;
using UnityEngine.InputSystem;

public class WallPassTrigger : MonoBehaviour
{
    public Collider2D TilemapCollider;
    public SpriteRenderer wallSprite;

    private const string PlayerTag = "Player";
    private bool inside = false;
    private AbilitySystem playerAbilities;
    private PlayerInputSimple playerInput;
    private Collider2D playerCollider;

    void Update()
    {
        if (!inside || playerAbilities == null || playerInput == null) return;

        if (playerAbilities.Has(AbilitySystem.Ability.WallPass))
        {
            if (Keyboard.current[playerInput.wallPassKey].isPressed)
                IgnoreCollision(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        wallSprite.sortingOrder = 51;
        inside = true;

        var go = other.gameObject;
        playerInput = go.GetComponent<PlayerInputSimple>();
        playerAbilities = go.GetComponent<AbilitySystem>();
        playerCollider = go.GetComponent<Collider2D>();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!inside || playerAbilities == null || playerInput == null) return;
        if (!other.CompareTag(PlayerTag)) return;

        if (playerAbilities.Has(AbilitySystem.Ability.WallPass))
        {
            if (Keyboard.current[playerInput.wallPassKey].isPressed)
                IgnoreCollision(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(PlayerTag)) return;

        wallSprite.sortingOrder = 0;
        inside = false;
        IgnoreCollision(false);

        playerInput = null;
        playerAbilities = null;
        playerCollider = null;
    }

    private void IgnoreCollision(bool ignore)
    {
        if (TilemapCollider == null || playerCollider == null) return;
        Physics2D.IgnoreCollision(playerCollider, TilemapCollider, ignore);
    }
}
