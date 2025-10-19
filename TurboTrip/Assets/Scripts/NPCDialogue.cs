using UnityEngine;
using TMPro;

public class NPCDialogue : MonoBehaviour
{
    [TextArea]
    public string dialogueText = "¡Hola Nick! Aquí no vendemos cigarros";
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueUIText;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ShowDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HideDialogue();
        }
    }

    private void ShowDialogue()
    {
        if (dialoguePanel != null && dialogueUIText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueUIText.text = dialogueText;
        }
    }

    private void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }
}