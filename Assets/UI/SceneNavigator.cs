
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigation : MonoBehaviour
{
    public void GoToMainMenu()
    {
        
        Time.timeScale = 1f;

   
        SceneManager.LoadScene(0);
    }
}