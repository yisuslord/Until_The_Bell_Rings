using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    public bool IsDialogueActive = false;

    public float textSpeed = 0.03f;

    private string[] currentLines;
    private int index;

    private void Awake()
    {
        Instance = this;
    }

    public void StartDialogue(string[] lines)
    {
        currentLines = lines;
        index = 0;

        IsDialogueActive = true;
        DialogueUI.Instance.dialogueText.text = "";
        DialogueUI.Instance.dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in currentLines[index])
        {
            DialogueUI.Instance.dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void Update()
    {
        if (DialogueUI.Instance.dialoguePanel.activeSelf && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.J)))
        {
            if (DialogueUI.Instance.dialogueText.text ==
                currentLines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                DialogueUI.Instance.dialogueText.text =
                    currentLines[index];
            }
        }
    }

    void NextLine()
    {
        if (index < currentLines.Length - 1)
        {
            index++;
            DialogueUI.Instance.dialogueText.text = "";
            StartCoroutine(TypeLine());
        }
        else
        {
            DialogueUI.Instance.HideDialogue();
            IsDialogueActive = false;
        }
    }
}