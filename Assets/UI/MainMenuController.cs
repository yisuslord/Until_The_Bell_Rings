using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private MainMenuView view;
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private string creditsSceneName = "Creditos"; // 🔥 Nombre de tu escena de Créditos

    [Header("Ajustes de Audio del Menú")]
    [SerializeField] private AudioClip mainMusicClip;
    [SerializeField] private float fadeOutDuration = 1.5f;

    private void Start()
    {
        if (AudioManager.Instance != null && mainMusicClip != null)
        {
            AudioManager.Instance.PlayMusic(mainMusicClip);
        }
    }

    private void OnEnable()
    {
        if (view != null)
        {
            view.OnStartPressed.AddListener(StartGame);
            view.OnQuitPressed.AddListener(QuitGame);
            view.OnCreditsPressed.AddListener(LoadCredits); // 🔥 NUEVO: Escuchamos el aviso de créditos
        }
    }

    private void OnDisable() // Es una buena práctica desvincularlos si el objeto se apaga
    {
        if (view != null)
        {
            view.OnStartPressed.RemoveListener(StartGame);
            view.OnQuitPressed.RemoveListener(QuitGame);
            view.OnCreditsPressed.RemoveListener(LoadCredits);
        }
    }

    private void StartGame()
    {
        StartCoroutine(SequenceStartGame());
    }

    private System.Collections.IEnumerator SequenceStartGame()
    {
        if (view != null) view.gameObject.SetActive(false);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.FadeOutMusic(fadeOutDuration);
        }

        yield return new WaitForSeconds(fadeOutDuration);

        LoadingManager.SceneToLoad = "Mapa";
        SceneManager.LoadScene("Loading");
    }

    // 🔥 NUEVA FUNCIÓN: Se ejecuta al presionar Créditos
    private void LoadCredits()
    {
        // Si quieres que la música se detenga al ir a créditos, descomenta la siguiente línea:
        // if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();

        SceneManager.LoadScene(creditsSceneName);
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