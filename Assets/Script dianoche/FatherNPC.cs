using UnityEngine;

public class FatherNPC : MonoBehaviour
{
    [SerializeField] private GameDayNightCycle dayNight;

    private bool playerNear = false;

    private void Update()
    {
        if (playerNear && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Iniciando Noche");
            dayNight.StartNight();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        playerNear = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        playerNear = false;
    }
}