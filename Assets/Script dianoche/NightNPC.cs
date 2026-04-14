using UnityEngine;

public class NightNPC : MonoBehaviour, IInteractable
{
    private bool playerIsNear = false;
    private bool canStartNight = false;

    public void Interact()
    {
        // Esto se llama cuando el jugador presiona E
        Debug.Log("Padre: 'Hijo, la oscuridad se acerca. ¿Estás listo? (Presiona J para empezar)'");
        canStartNight = true;
    }

    private void Update()
    {
        // Si ya hablamos con él y presionamos J, empieza la noche
        if (canStartNight && Input.GetKeyDown(KeyCode.J) && LevelManager.Instance.currentState == GameState.Day)
        {
            LevelManager.Instance.StartNight();
            canStartNight = false; // Resetear para la próxima vez
        }
    }
}