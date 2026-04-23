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
        dialoguePanel.SetActive(false);
    }

    public void ShowDialogue(string dialogue)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = dialogue;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
    }

    public bool IsOpen
    {
        get { return dialoguePanel.activeSelf; }
    }

}