using UnityEngine;
using UnityEngine.UI;

public class OffScreenWarning : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject indicatorPrefab; // El objeto con el icono (!)
    [SerializeField] private float margin = 50f; // Espacio desde el borde de la pantalla

    private GameObject indicatorInstance;
    private RectTransform indicatorRect;
    private Candle candle;
    private Camera mainCamera;
    private CanvasGroup canvasGroup;

    private void Start()
    {
        candle = GetComponent<Candle>();
        mainCamera = Camera.main;

        // Instanciamos el indicador dentro del Canvas
        GameObject canvas = GameObject.Find("Canvas"); // Asegúrate de que tu Canvas se llame así
        indicatorInstance = Instantiate(indicatorPrefab, canvas.transform);
        indicatorRect = indicatorInstance.GetComponent<RectTransform>();
        canvasGroup = indicatorInstance.GetComponent<CanvasGroup>();

        indicatorInstance.SetActive(false);
    }

    private void Update()
    {
        // REGLA 1: Solo mostrar si la vela está apagada/corrupta
        if (candle.IsLit && !candle.IsCorrupted)
        {
            indicatorInstance.SetActive(false);
            return;
        }

        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        // REGLA 2: ¿Está fuera de la pantalla?
        // screenPos.z > 0 asegura que el objeto no esté detrás de la cámara
        bool isOffScreen = screenPos.x <= 0 || screenPos.x >= Screen.width ||
                           screenPos.y <= 0 || screenPos.y >= Screen.height;

        if (isOffScreen)
        {
            indicatorInstance.SetActive(true);

            // Hacer que parpadee (Intermitente)
            float alpha = Mathf.PingPong(Time.time * 5f, 1f);
            canvasGroup.alpha = alpha;

            // Limitar la posición al borde de la pantalla con el margen
            float edgeX = Mathf.Clamp(screenPos.x, margin, Screen.width - margin);
            float edgeY = Mathf.Clamp(screenPos.y, margin, Screen.height - margin);

            indicatorRect.position = new Vector2(edgeX, edgeY);
        }
        else
        {
            // Si la vela se ve en pantalla, apagamos el indicador 
            // (porque el jugador ya está viendo la vela físicamente)
            indicatorInstance.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (indicatorInstance != null) Destroy(indicatorInstance);
    }
}