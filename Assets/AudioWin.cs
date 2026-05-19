using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class AudioWin : MonoBehaviour
{
    [Header("Audio de Victoria")]
    [SerializeField] private AudioClip musicaVictoria; // 🔥 Arrastra aquí tu pista de celebración/créditos

    [Header("UI del Menú")]
    [SerializeField] private Button botonMenuPrincipal; // Arrastra el botón de la interfaz

    private void Start()
    {
        // 1. En cuanto la escena inicia, le ordenamos al AudioManager que ponga la música de victoria
        if (AudioManager.Instance != null && musicaVictoria != null)
        {
            AudioManager.Instance.PlayMusic(musicaVictoria);
        }
        else if (AudioManager.Instance == null)
        {
            Debug.LogWarning("[VictoryScene] No se encontró el AudioManager en la escena. ¿Iniciaste el juego desde el Menú Principal?");
        }

        // 2. Configurar el botón para regresar al menú por código
        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.onClick.AddListener(RegresarAlMenu);
        }
    }

    private void RegresarAlMenu()
    {
        // Antes de irnos, podemos apagar la música de victoria si queremos que el menú principal empiece limpio
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        // Cargamos la escena del menú principal (asegúrate de que el nombre coincida con tu escena real)
        SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        // Buena práctica: Limpiamos el listener al destruir la escena para evitar fugas de memoria
        if (botonMenuPrincipal != null)
        {
            botonMenuPrincipal.onClick.RemoveAllListeners();
        }
    }
}