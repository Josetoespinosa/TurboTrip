using UnityEngine;
using TMPro;

public class NPCSmallTalk : MonoBehaviour
{
    [Header("Diálogo")]
    [TextArea(2, 5)]
    public string[] lines = {
        "Hola Nick, ¿otra vez por aquí?",
        "Recuerda: no vendas cigarros en el colegio.",
        "Bueno, eso, suerte."
    };

    [Tooltip("Si está activo, cuando termine el diálogo vuelve a la primera línea para poder repetirlo.")]
    public bool loopDialogue = false;

    [Header("UI (si las dejas vacías, se buscarán por nombre)")]
    public GameObject dialoguePanel;      // Canvas/DialogPanel
    public TMP_Text dialogueUIText;       // Canvas/DialoguePanel/TextMeshProUGUI
    public GameObject interactPanel;      // Canvas/InteractPanel

    [Header("Interacción")]
    public bool useTrigger = true;        // recomendado (OnTrigger)
    public float talkRadius = 1.6f;       // usado sólo si useTrigger=false
    public LayerMask playerMask = ~0;     // usado sólo si useTrigger=false

    [Header("Depuración")]
    public bool debugLogs = false;

    // Estado interno
    private bool isPlayerNear = false;
    private bool isDialogueActive = false;
    private int lineIndex = 0;

    private PlayerInputSimple playerInput;
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
        // Alternativa sin trigger: detección por radio
        if (!useTrigger)
        {
            Collider2D p = Physics2D.OverlapCircle(transform.position, talkRadius, playerMask);
            if (p != null && !isPlayerNear) OnPlayerEnter(p);
            else if (p == null && isPlayerNear) OnPlayerExit();
        }

        if (!isPlayerNear || playerInput == null) return;

        // Tecla de interacción definida en PlayerInputSimple (por ejemplo E)
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
            if (debugLogs) Debug.Log("[NPCSmallTalk] Mostrar diálogo (línea 0)");
        }
        else if (debugLogs) Debug.LogWarning("[NPCSmallTalk] Falta asignar DialoguePanel/DialogueUIText");
    }

    private void Advance()
    {
        lineIndex++;

        if (lines != null && lineIndex < lines.Length)
        {
            if (dialogueUIText) dialogueUIText.text = lines[lineIndex];
            if (debugLogs) Debug.Log("[NPCSmallTalk] Avanzar diálogo (línea " + lineIndex + ")");
        }
        else
        {
            // Fin del diálogo
            if (dialoguePanel) dialoguePanel.SetActive(false);
            isDialogueActive = false;

            if (loopDialogue)
            {
                // Resetea el índice para que pueda comenzar de nuevo en la próxima interacción
                lineIndex = 0;
            }

            // Si el player sigue cerca, vuelve a mostrar el cartel de "Pulsa E"
            if (interactPanel && isPlayerNear) interactPanel.SetActive(true);
        }
    }

    // ---- Trigger ----
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;

        var input = other.GetComponent<PlayerInputSimple>();
        if (input == null) return;

        OnPlayerEnter(other);
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

        if (!isDialogueActive && interactPanel) interactPanel.SetActive(true);
        if (debugLogs) Debug.Log("[NPCSmallTalk] Player cerca (ENTER)");
    }

    private void OnPlayerExit()
    {
        isPlayerNear = false;
        playerInput = null;
        playerCollider = null;

        if (interactPanel) interactPanel.SetActive(false);
        if (dialoguePanel) dialoguePanel.SetActive(false);
        isDialogueActive = false;
        lineIndex = 0;

        if (debugLogs) Debug.Log("[NPCSmallTalk] Player se alejó (EXIT)");
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
