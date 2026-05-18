using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Settings")]
    [SerializeField] private float minimumLoadingTime = 10f; // 🔥 Forzamos los 10 segundos mínimos

    public static string SceneToLoad = "Mapa";

    private void Start()
    {
        StartCoroutine(LoadSceneAsyncCoroutine());
    }

    private IEnumerator LoadSceneAsyncCoroutine()
    {
        // 1. Iniciamos la carga real en segundo plano
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneToLoad);
        operation.allowSceneActivation = false; // Impedimos que se active sola

        float timeElapsed = 0f;

        // 2. Bucle principal controlado por tiempo y por la carga de Unity
        while (timeElapsed < minimumLoadingTime || operation.progress < 0.9f)
        {
            timeElapsed += Time.deltaTime;

            // Calculamos dos progresos diferentes:
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f); // Progreso real de carga (0 a 1)
            float timeProgress = Mathf.Clamp01(timeElapsed / minimumLoadingTime); // Progreso del tiempo (0 a 1)

            // Usamos el valor MENOR de los dos para la barra. 
            // Así, si el juego ya cargó al 100% real, la barra visual avanzará al ritmo de los 10 segundos.
            float visualProgress = Mathf.Min(realProgress, timeProgress);

            // Actualizamos la interfaz con el progreso visual suavizado
            if (progressBar != null) progressBar.value = visualProgress;
            if (progressText != null) progressText.text = $"Cargando... {Mathf.RoundToInt(visualProgress * 100)}%";

            yield return null; // Esperamos al siguiente frame
        }

        // 3. Cuando pasaron los 10 segundos Y la escena está completamente lista en memoria
        if (progressBar != null) progressBar.value = 1f;

        if (progressText != null)
            progressText.text = "¡Listo! Presiona cualquier tecla para continuar...";

        // Esperamos a que el jugador presione una tecla para entrar a la acción
        while (!Input.anyKey)
        {
            yield return null;
        }

        // Activamos el nivel
        operation.allowSceneActivation = true;
    }
}