// Guardar como: DeathMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathMenuController : MonoBehaviour
{

    private void Start()
    {
        // Al entrar a la escena de muerte, nos aseguramos de que el tiempo
        // corra normalmente por si venimos de un Time.timeScale = 0
        Time.timeScale = 1f;

        // Aseguramos que el cursor sea visible para poder interactuar con los botones
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Recarga la escena del juego para volver a intentar.
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("Hola");
        // Opcional: Si usas algún sistema de puntuación o estado global, resetealo aquí
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// Regresa al menú principal (Escena 0).
    /// </summary>
    public void ExitToMenu()
    {
        // Cargamos por índice para mayor seguridad (0 suele ser el Menú Principal)
        SceneManager.LoadScene(0);
    }
}