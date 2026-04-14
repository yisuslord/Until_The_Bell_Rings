// Guardar como: PauseController.cs
using UnityEngine;

public class PauseController : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenuCanvas;
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    public bool IsPaused { get; private set; }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (IsPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // Congela el mundo físico y animaciones
        pauseMenuCanvas.SetActive(true);
        Cursor.lockState = CursorLockMode.None; // Libera el mouse para el menú
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f; // Reanuda el tiempo
        pauseMenuCanvas.SetActive(false);
    }
}