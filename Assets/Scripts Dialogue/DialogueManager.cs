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

    // Activa el sistema de dialogo y empieza a mostrar el paquete de frases que le pasemos
    public void StartDialogue(string[] lines)
    {
        currentLines = lines;
        index = 0;

        IsDialogueActive = true;
        DialogueUI.Instance.dialogueText.text = "";
        DialogueUI.Instance.dialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    // Efecto de maquina de escribir: va sumando letra por letra con un pequeño retraso
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
        // Si el panel de dialogo esta en pantalla y el jugador presiona un boton de accion
        if (DialogueUI.Instance.dialoguePanel.activeSelf && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.J) || Input.GetButtonDown("Interact") || Input.GetButtonDown("Select")))
        {
            // Si el texto ya se termino de escribir completo, pasa a la siguiente linea
            if (DialogueUI.Instance.dialogueText.text == currentLines[index])
            {
                NextLine();
            }
            // Si el texto todavia se esta escribiendo, frena el efecto y muestra la frase completa de golpe
            else
            {
                StopAllCoroutines();
                DialogueUI.Instance.dialogueText.text = currentLines[index];
            }
        }
    }

    // Controla si pasamos a la siguiente frase o si ya cerramos la conversacion
    void NextLine()
    {
        // Si quedan mas frases en la lista, avanzamos una posicion y volvemos a activar el efecto
        if (index < currentLines.Length - 1)
        {
            index++;
            DialogueUI.Instance.dialogueText.text = "";
            StartCoroutine(TypeLine());
        }
        // Si ya no hay mas frases, ocultamos el panel y avisamos que el dialogo termino
        else
        {
            DialogueUI.Instance.HideDialogue();
            IsDialogueActive = false;
        }
    }
}