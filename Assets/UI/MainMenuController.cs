// Guardar como: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private MainMenuView view;
    [SerializeField] private string gameSceneName = "GameScene";

    private void OnEnable()
    {
        if (view != null)
        {
            view.OnStartPressed.AddListener(StartGame);
            view.OnQuitPressed.AddListener(QuitGame);
        }
    }

    private void StartGame()
    {
        // Al usar SceneManager, Unity carga la nueva escena
        SceneManager.LoadScene(gameSceneName);
    }

    private void QuitGame()
    {
        Debug.Log("Saliendo...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}