using UnityEngine;

public class NightNPC : MonoBehaviour, IInteractable
{
    private bool canStartNight = false;

    public void Interact()
    {
        Debug.Log("Padre: 'Hijo, la oscuridad se acerca. ¿Estás listo? (Presiona J para empezar)'");
        canStartNight = true;
    }

    private void Update()
    {
        if (canStartNight && Input.GetKeyDown(KeyCode.J) && LevelManager.Instance.currentState == GameState.Day)
        {
            LevelManager.Instance.StartNight();
            canStartNight = false;
        }
    }
}