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

    [Header("Audio de Advertencia")]
    [SerializeField] private AudioClip clipAdvertencia; // 🔥 Arrastra el sonido aquí
    [SerializeField] private bool usarSonidoEnBucle = false; // ¿Suena constante o solo un golpe?
    private AudioSource canalSonidoActivo; // Guarda la referencia si se reproduce en loop para poder apagarlo después

    // 🔥 EL CANDADO: Evita que el sonido se ejecute infinitamente en el Update
    private bool isSoundPlaying = false;

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
            if (indicatorInstance.activeSelf)
            {
                indicatorInstance.SetActive(false);
            }

            // 🔥 Si la vela se enciende o se limpia, apagamos el audio inmediatamente
            ApagarSonidoAdvertencia();
            return;
        }

        UpdateIndicator();
    }

    private void UpdateIndicator()
    {
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        // REGLA 2: ¿Está fuera de la pantalla?
        bool isOffScreen = screenPos.x <= 0 || screenPos.x >= Screen.width ||
                           screenPos.y <= 0 || screenPos.y >= Screen.height;

        if (isOffScreen)
        {
            indicatorInstance.SetActive(true);

            // 🔥 ¡AQUÍ SE ENCIENDE EL AUDIO! Solo entra si el sonido no se está reproduciendo ya
            EncenderSonidoAdvertencia();

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
            indicatorInstance.SetActive(false);

            // 🔥 Si entra a la pantalla, el peligro visual pasa, por lo que silenciamos el audio
            ApagarSonidoAdvertencia();
        }
    }

    // 🔥 MÉTODOS AUXILIARES DE CONTROL DE AUDIO INTERNO 🔥

    private void EncenderSonidoAdvertencia()
    {
        if (isSoundPlaying) return; // Si ya está sonando, ignoramos

        if (AudioManager.Instance != null && clipAdvertencia != null)
        {
            isSoundPlaying = true;

            if (usarSonidoEnBucle)
            {
                // 🔥 Llamamos al nuevo método que acabamos de crear
                canalSonidoActivo = AudioManager.Instance.PlaySFXLoop(clipAdvertencia, 1f);
            }
            else
            {
                // Un único golpe de sonido de alerta en 2D
                AudioManager.Instance.PlaySFX2D(clipAdvertencia, 1f);
            }
        }
    }

    private void ApagarSonidoAdvertencia()
    {
        if (!isSoundPlaying) return; // Si ya estaba apagado, salimos

        isSoundPlaying = false;

        if (usarSonidoEnBucle && canalSonidoActivo != null)
        {
            canalSonidoActivo.Stop();

            // 🔥 Como el AudioManager creó un GameObject para este loop, 
            // destruimos el objeto contenedor para limpiar la escena.
            Destroy(canalSonidoActivo.gameObject);
            canalSonidoActivo = null;
        }
    }

    private void OnDestroy()
    {
        // Seguridad: Si el objeto se destruye, matamos el sonido también
        ApagarSonidoAdvertencia();

        if (indicatorInstance != null) Destroy(indicatorInstance);
    }
}