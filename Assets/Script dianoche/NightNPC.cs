using UnityEngine;
using System.Collections;

public class NightNPC : MonoBehaviour, IInteractable
{
    private bool canStartNight = false;

    // Estado de poder o no interactuar para evitar interaccion extra
    public bool canInteract = true;

    // Fase de los dialogos (Que dialogo va a decir)
    private int dialogueStep = 0;



    // Dialogos
    private string[][] dialogues =
    {
        // Bienvenida + Movimiento
        new string[]
        {
            "PADRE: Bienvenido, tu debes ser el nuevo sacristán, Samuel verdad?",
            "Yo soy el padre de esta parroquia, con más de 30 años de experiencia, supongo que ya conoces tu función, deberás cuidar y preservar el templo",
            "Cómo lo harás? Yo te explicaré eso",
            "Primero que nada puedes empezar por recorrer el lugar tranquilamente o si tienes prisa puedes correr (A)",
            "A lo largo de la parroquia hay varias cosas con las cuales interactuar, guardar o incluso esconderte en ellas (B)",
            "Si las recoges puedes verlas en tu inventario y usarlas en el momento que quieras (X) siempre que las hayas elegido (LB, RB)",
            "Adelante investiga un poco el templo y vuelve conmigo cuando estés listo."
        },

        // Explicacion enemigos
        new string[]
        {
            "PADRE: Bien, ahora te contare un pequeño secreto, cuidar del altar no es tan simple como parece, deberás estar muy atento en las noches",
            "Este lugar lleva mucho tiempo aquí y es muy especial, cuando la oscuridad cae hay criaturas intentando dañar el altar y es tu deber evitarlo" +
            ", hasta ahora hemos identificado 3 de ellas: ",
            "El Sensible: Patrulla en las noches y si percibe luz o sonido te perseguirá, te recomiendo no correr tanto y si te persigue escóndete (B en un escondite) para que se vaya.",
            "El Manifestado: Aparece en zonas sin luz cuando estás mucho tiempo ahí, regularmente desaparece cuando te ataca",
            "El Corruptor: El apaga las velas y corrompe las reliquias, dejandolas inútiles por un tiempo",
            "Tu función será la de evitar que todas las velas se apaguen, cuando una esté apagada acércate y enciendela (B), pero evita que te hagan daño.",
            "Eso es todo lo que necesitas saber por ahora, explora el entorno y habla conmigo cuando estés listo para enfrentar la noche"
        },

        // Explicación objetos
        new string[]
        {
            "PADRE: A lo largo de la iglesia, encontrarás objetos que te ayudarán a cumplir tu misión",
            "Las baterías: tendrás una linterna cuya energía se agota, pero descuida, mientras tengas baterías contigo estarás bien",
            "Los cálices: si un demonio te lastima, un cáliz podrá ayudarte a sanar",
            "Las cruces: serán tu protección contra el sensible, pues éste no tolera el poder de Dios"
        },

        // Empezar Noche
        new string[]
        {
            "PADRE: Estas Listo?",
            "Estas protegiendo un templo sagrado, ten cuidado la noche se acerca",
            "(Presiona Select para empezar)"
        }
    };


    public void Interact()
    {
        if (DialogueUI.Instance.IsOpen || !canInteract || LevelManager.Instance.currentState == GameState.Night)
            return;

        Debug.Log("El PADRE ESTA HABLANDO");
        int indexToUse = Mathf.Min(dialogueStep, dialogues.Length - 1);

        DialogueManager.Instance.StartDialogue(
            dialogues[indexToUse]
        );

        if (indexToUse == dialogues.Length - 1)
        {
            canStartNight = true;
        }
        else
        {
            dialogueStep++;
        }

        StartCoroutine(WaitForDialogueEnd());

    }

    private void Update()
    {
        if (canStartNight && (Input.GetKeyDown(KeyCode.J)||Input.GetButtonDown("Select")) && LevelManager.Instance.currentState == GameState.Day)
        {
            LevelManager.Instance.StartNight();
            canInteract = false;
            canStartNight = false;

        }

        if (LevelManager.Instance.currentState == GameState.Day && !canInteract)
        {
            canInteract = true;
        }
    }

    IEnumerator WaitForDialogueEnd()
    {
        canInteract = false;

        yield return new WaitUntil(() =>
            !DialogueManager.Instance.IsDialogueActive);

        yield return new WaitForSeconds(2f);

        canInteract = true;
    }

}