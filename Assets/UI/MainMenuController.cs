// Guardar como: MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private MainMenuView view;
    [SerializeField] private string gameSceneName = "GameScene";

    [Header("Ajustes de Audio del Menú")]
    [SerializeField] private AudioClip mainMusicClip; // 🔥 Asigna tu pista del menú en el Inspector
    [SerializeField] private float fadeOutDuration = 1.5f; // Cuánto tardará en apagarse la música

    private void Start()
    {
        // En cuanto arranca el menú principal, le ordenamos al AudioManager que ponga la música en bucle
        if (AudioManager.Instance != null && mainMusicClip != null)
        {
            // Nota: Usa el método que tengas en tu AudioManager para reproducir música en loop, ej: PlayMusic(mainMusicClip, true);
            AudioManager.Instance.PlayMusic(mainMusicClip);
        }
    }

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

        // 🔥 En lugar de cargar directo, iniciamos la secuencia de transición con el Fade Out
        StartCoroutine(SequenceStartGame());
    }

    // 🔥 LA SECUENCIA DE ESPERA: Controla el tiempo antes de saltar a la pantalla de carga
    private System.Collections.IEnumerator SequenceStartGame()
    {
        // 1. Desactivamos los botones del menú en la View para que el jugador no pueda spamear el click mientras ocurre el fade
        if (view != null)
        {
            view.gameObject.SetActive(false); // O apagar los botones individualmente para que no haya interrupciones
        }

        // 2. Le ordenamos al AudioManager que inicie el desvanecimiento de la pista
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(fadeOutDuration);
        }

        // 3. Frenamos este script por la cantidad de segundos que dura el Fade Out
        yield return new WaitForSeconds(fadeOutDuration);

        // 4. Una vez que todo está en silencio... ¡Cargamos las escenas de juego!
        LoadingManager.SceneToLoad = "Mapa";
        SceneManager.LoadScene("Loading");
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