using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("Indicator")]
    public GameObject activeIndicator;
    public Sprite inactiveSprite;
    public Sprite activeSprite;

    private bool isActive = false;
    private PlayerRespawnManager respawnManager;
    private SpriteRenderer indicatorSR;

    private void Awake()
    {
        // Ensure trigger collider
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void Start()
    {
        respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
        if (activeIndicator == null)
        {
            // Create a simple indicator if none assigned
            activeIndicator = new GameObject("ActiveIndicator");
            activeIndicator.transform.SetParent(transform);
            activeIndicator.transform.localPosition = Vector3.up * 1.2f;
            indicatorSR = activeIndicator.AddComponent<SpriteRenderer>();
            indicatorSR.color = Color.yellow;
            indicatorSR.sortingOrder = 10;
            // The sprite will be null unless assigned in editor; users can override later.
        }
        else
        {
            indicatorSR = activeIndicator.GetComponent<SpriteRenderer>();
            if (indicatorSR == null)
            {
                indicatorSR = activeIndicator.AddComponent<SpriteRenderer>();
            }
        }

        // Initialize indicator visual state
        if (indicatorSR != null && inactiveSprite != null)
        {
            // Show inactive sprite if provided
            indicatorSR.sprite = inactiveSprite;
            activeIndicator.SetActive(true);
        }
        else
        {
            // Fallback to hidden indicator until activated
            activeIndicator.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isActive) return;
        if (!collision.CompareTag("Player")) return;
        if (respawnManager == null)
        {
            respawnManager = FindAnyObjectByType<PlayerRespawnManager>();
            if (respawnManager == null) return;
        }

        respawnManager.SetCheckpoint(transform);
        isActive = true;
        if (activeIndicator != null)
        {
            if (indicatorSR != null && activeSprite != null)
            {
                indicatorSR.sprite = activeSprite;
                activeIndicator.SetActive(true);
            }
            else
            {
                // If no active sprite set, at least show the indicator
                activeIndicator.SetActive(true);
            }
        }
    }
}
