
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void RestartGame()
    {
        Time.timeScale = 1f;
        // Cargamos la escena del juego por nombre o por índice
        SceneManager.LoadScene(1);
    }
    public void GoToMainMenu()
    {
        
        Time.timeScale = 1f;

   
        SceneManager.LoadScene(0);
    }
}