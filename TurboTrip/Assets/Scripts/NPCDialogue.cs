using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; 

public class NPCDialogue : MonoBehaviour
{
    [TextArea]
    public string dialogueText = "¡Hola Nick! Aquí no vendemos cigarros. \n \n Pulsa 'G' para continuar.";

    public GameObject dialoguePanel;      
    public TMP_Text dialogueUIText;       
    public GameObject interactPanel;      

    [Header("Cambio de escena")]
    public string nextSceneName; 

    private bool isPlayerNear = false;
    private bool isDialogueActive = false;

    private void Start()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (interactPanel != null)
            interactPanel.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (isPlayerNear && !isDialogueActive && Keyboard.current.gKey.wasPressedThisFrame)
        {
            ShowDialogue();
        }
        else if (isDialogueActive && Keyboard.current.gKey.wasPressedThisFrame)
        {
            ChangeScene();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            if (interactPanel != null)
                interactPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
            if (interactPanel != null)
                interactPanel.SetActive(false);
            HideDialogue();
        }
    }

    private void ShowDialogue()
    {
        if (dialoguePanel != null && dialogueUIText != null)
        {
            dialoguePanel.SetActive(true);
            dialogueUIText.text = dialogueText;
            isDialogueActive = true;

            if (interactPanel != null)
                interactPanel.SetActive(false);
        }
    }

    private void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
        isDialogueActive = false;
    }

    private void ChangeScene()
    {
        HideDialogue();

        if (LevelTimer.Instance != null)
        {
            LevelTimer.Instance.FinishLevel();
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}