using UnityEngine;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private void Awake()
    {
        Instance = this;

        // Nos aseguramos de que el panel de dialogo empiece oculto al iniciar el juego
        dialoguePanel.SetActive(false);
    }

    // Muestra el panel en pantalla e inyecta directamente el texto que le pasemos
    public void ShowDialogue(string dialogue)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogue;
    }

    // Oculta el panel de dialogo de la interfaz
    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    // Propiedad para que otros scripts puedan consultar rapidamente si el panel esta abierto o cerrado
    public bool IsOpen
    {
        get { return dialoguePanel.activeSelf; }
    }
}