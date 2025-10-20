using UnityEngine;
using UnityEngine.InputSystem;

public class WallPassTrigger : MonoBehaviour
{

    public Collider2D TilemapCollider;
    public SpriteRenderer wallSprite;

    private const string PlayerTag = "Player";
    private bool activatedInput = false;

    void Update()
    {
        if (Keyboard.current.qKey.isPressed)
        {
            if (activatedInput)
            {
                IgnoreCollision(true);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(PlayerTag))
        {
            wallSprite.sortingOrder = 51;
            activatedInput = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(PlayerTag))
        {
            if (Keyboard.current.qKey.isPressed)
            {
                IgnoreCollision(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(PlayerTag))
        {
            wallSprite.sortingOrder = 0;
            activatedInput = false;

            IgnoreCollision(false);
        }
    }

    private void IgnoreCollision(bool ignore)
    {
        if (TilemapCollider != null)
        {
            Collider2D playerCollider = GameObject.FindGameObjectWithTag(PlayerTag)?.GetComponent<Collider2D>();

            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, TilemapCollider, ignore);
            }
        }
    }
}
