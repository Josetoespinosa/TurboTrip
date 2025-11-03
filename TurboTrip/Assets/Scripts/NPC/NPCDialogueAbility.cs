using UnityEngine;
using TMPro;

public class NPCDialogueAbility : MonoBehaviour
{
    [Header("Diálogo")]
    [TextArea(2, 5)]
    public string[] lines = { "Hola.", "Pulsa E para continuar.", "¡Listo!" };

    [Header("UI (si las dejas vacías, se buscarán por nombre)")]
    public GameObject dialoguePanel;      // Canvas/DialogPanel
    public TMP_Text dialogueUIText;       // Canvas/DialoguePanel/TextMeshProUGUI
    public GameObject interactPanel;      // Canvas/InteractPanel

    [Header("Desbloqueo de habilidad")]
    public bool unlockAbilityOnFinish = true;
    public AbilitySystem.Ability abilityToUnlock = AbilitySystem.Ability.DoubleJump;

    [Header("Interacción")]
    public bool useTrigger = true;        // recomendado (OnTrigger)
    public float talkRadius = 1.6f;       // usado sólo si useTrigger=false
    public LayerMask playerMask = ~0;     // usado sólo si useTrigger=false

    [Header("Depuración")]
    public bool debugLogs = false;

    // Estado
    private bool isPlayerNear = false;
    private bool isDialogueActive = false;
    private int lineIndex = 0;

    private PlayerInputSimple playerInput;      // referencia al Player
    private AbilitySystem playerAbilities;
    private Collider2D playerCollider;

    void Start()
    {
        // Autobuscar paneles si no están asignados
        if (!dialoguePanel)
        {
            var dp = GameObject.Find("DialoguePanel");
            if (dp) dialoguePanel = dp;
        }
        if (!interactPanel)
        {
            var ip = GameObject.Find("InteractPanel");
            if (ip) interactPanel = ip;
        }
        if (!dialogueUIText && dialoguePanel)
        {
            dialogueUIText = dialoguePanel.GetComponentInChildren<TMP_Text>(true);
        }

        if (dialoguePanel) dialoguePanel.SetActive(false);
        if (interactPanel) interactPanel.SetActive(false);
    }

    void Update()
    {
        // Alternativa sin trigger
        if (!useTrigger)
        {
            Collider2D p = Physics2D.OverlapCircle(transform.position, talkRadius, playerMask);
            if (p != null && !isPlayerNear) OnPlayerEnter(p);
            else if (p == null && isPlayerNear) OnPlayerExit();
        }

        if (!isPlayerNear || playerInput == null) return;

        // Tecla de interacción (definida en PlayerInputSimple)
        if (!isDialogueActive && playerInput.interactPressed)
        {
            ShowFirstLine();
        }
        else if (isDialogueActive && playerInput.interactPressed)
        {
            Advance();
        }
    }

    private void ShowFirstLine()
    {
        lineIndex = 0;
        if (dialoguePanel && dialogueUIText)
        {
            dialoguePanel.SetActive(true);
            dialogueUIText.text = (lines != null && lines.Length > 0) ? lines[lineIndex] : "";
            isDialogueActive = true;
            if (interactPanel) interactPanel.SetActive(false);
            if (debugLogs) Debug.Log("[NPC] Mostrar diálogo (línea 0)");
        }
        else if (debugLogs) Debug.LogWarning("[NPC] Falta asignar DialoguePanel/DialogueUIText");
    }

    private void Advance()
    {
        lineIndex++;
        if (lines != null && lineIndex < lines.Length)
        {
            if (dialogueUIText) dialogueUIText.text = lines[lineIndex];
            if (debugLogs) Debug.Log("[NPC] Avanzar diálogo (línea " + lineIndex + ")");
        }
        else
        {
            // fin
            if (dialoguePanel) dialoguePanel.SetActive(false);
            isDialogueActive = false;

            // Desbloqueo
            if (unlockAbilityOnFinish && playerAbilities != null)
            {
                bool unlocked = playerAbilities.Unlock(abilityToUnlock);
                if (debugLogs) Debug.Log(unlocked
                    ? $"[NPC] Desbloqueado: {abilityToUnlock}"
                    : $"[NPC] Ya estaba desbloqueada: {abilityToUnlock}");
            }

            // Mostrar prompt si sigue cerca
            if (interactPanel && isPlayerNear) interactPanel.SetActive(true);
        }
    }

    // ---- Trigger ----
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;

        // En vez de Tag, detectamos por componente:
        var input = other.GetComponent<PlayerInputSimple>();
        if (input == null) return;

        OnPlayerEnter(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!useTrigger) return;

        // Nada especial; Update maneja la tecla
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!useTrigger) return;

        var input = other.GetComponent<PlayerInputSimple>();
        if (input == null) return;

        OnPlayerExit();
    }

    // ---- Helpers ----
    private void OnPlayerEnter(Collider2D playerCol)
    {
        isPlayerNear = true;
        playerCollider = playerCol;

        var go = playerCol.gameObject;
        playerInput = go.GetComponent<PlayerInputSimple>();
        playerAbilities = go.GetComponent<AbilitySystem>();

        if (!isDialogueActive && interactPanel) interactPanel.SetActive(true);
        if (debugLogs) Debug.Log("[NPC] Player cerca (ENTER)");
    }

    private void OnPlayerExit()
    {
        isPlayerNear = false;
        playerInput = null;
        playerAbilities = null;
        playerCollider = null;

        if (interactPanel) interactPanel.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(false);
        isDialogueActive = false;
        lineIndex = 0;

        if (debugLogs) Debug.Log("[NPC] Player se alejó (EXIT)");
    }

    void OnDrawGizmosSelected()
    {
        if (!useTrigger)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, talkRadius);
        }
    }
}
