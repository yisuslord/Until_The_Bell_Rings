using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // NECESARIO para controlar Light 2D
using TMPro; // 🔥 NECESARIO para controlar el componente de Texto

public class Candle : MonoBehaviour, IInteractable, ICorruptible
{
    [Header("Initial State")]
    [SerializeField] private bool startLit = false;

    [Header("Settings")]
    [SerializeField] private float lightRadius = 8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float corruptionDuration = 60f;

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color litColor = Color.white;
    [SerializeField] private Color unlitColor = Color.gray;

    [Header("Real Light")]
    [SerializeField] private Light2D candleLight;
    [SerializeField] private float lightIntensity = 1.0f; // Intensidad cuando está prendida

    [Header("UI de Interacción (Auto-asignada)")]
    private TextMeshProUGUI textoInteraccionUI;
    private bool playerInRange = false;

    private bool isLit = false;
    private bool isCorrupted = false;
    private Altar altar;

    public bool IsCorrupted => isCorrupted;
    public bool IsLit => isLit;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

        // Intentamos buscar la luz en los hijos si no se asignó en el inspector
        if (candleLight == null) candleLight = GetComponentInChildren<Light2D>();

        // 🛑 CANDADO DE INICIALIZACIÓN: Establecemos el estado lógico ANTES del primer frame
        if (startLit)
        {
            isLit = true;
        }

        // 🔥 LA BÚSQUEDA BLINDADA: Escanea la escena buscando el objeto "TextoInteraccion"
        // sin importar si está encendido o apagado por otros elementos del Canvas.
        TextMeshProUGUI[] todosLosTextos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (var texto in todosLosTextos)
        {
            if (texto.gameObject.name == "TextoInteraccion")
            {
                textoInteraccionUI = texto;
                break; // Lo encontramos, salimos del bucle
            }
        }

        // Limpieza inicial de seguridad
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "";
        }
        else
        {
            Debug.LogWarning($"[Candle] No se encontró el GameObject 'TextoInteraccion' en la escena para {gameObject.name}");
        }
    }

    private void Start()
    {
        altar = Object.FindFirstObjectByType<Altar>();

        // Sincronizamos los gráficos y componentes lumínicos con el estado asignado en el Awake
        UpdateVisuals();

        if (isLit)
        {
            // Notificamos a los sistemas del mapa que ya nació encendida
            NotifyAltar();
            EmitStimulus(StimulusType.Light);
            EmitStimulus(StimulusType.Corruptible);
            Debug.Log($"{gameObject.name}: Inicializada Encendida con Luz Real");
        }
        else
        {
            // Si empieza apagada, nos aseguramos de que el texto esté oculto al arrancar el nivel
            OcultarTexto();
        }
    }

    public void Interact()
    {
        // Solo permitimos encenderla si está apagada
        if (!isLit)
        {
            isCorrupted = false;
            LightCandle();

            // 🔥 Ocultamos el texto inmediatamente al encenderse con éxito
            OcultarTexto();
        }
    }

    private void LightCandle()
    {
        isLit = true;
        Debug.Log($"{gameObject.name}: Encendida con Luz Real");

        UpdateVisuals();
        NotifyAltar();

        EmitStimulus(StimulusType.Light);
        EmitStimulus(StimulusType.Corruptible);
    }

    public void Corrupt()
    {
        if (isCorrupted) return;

        isCorrupted = true;
        isLit = false;
        Debug.Log("<color=purple>Vela apagada y luz desactivada por Corruptor</color>");

        UpdateVisuals();
        NotifyAltar();

        // 🔥 Si el jugador estaba parado al lado de la vela cuando el Corruptor la apagó,
        // volvemos a mostrar el texto "Encender Vela"
        if (playerInRange)
        {
            MostrarTexto();
        }

        StartCoroutine(RestoreTimer());
    }

    public void Restore()
    {
        isCorrupted = false;
        UpdateVisuals();
        NotifyAltar();
    }

    private void NotifyAltar() => altar?.NotifyCandleChanged();

    private IEnumerator RestoreTimer()
    {
        yield return new WaitForSeconds(corruptionDuration);
        Restore();
    }

    private void EmitStimulus(StimulusType type)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, lightRadius, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
                receiver.OnStimulusReceived(transform.position, type);
        }
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isLit ? litColor : unlitColor;
        }

        if (candleLight != null)
        {
            candleLight.enabled = isLit;
            candleLight.intensity = isLit ? lightIntensity : 0f;
        }
    }

    // --- DETECCIÓN EN RANGO PARA LA INTERFAZ DE USUARIO ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

            // 🔥 Solo mostramos el texto si la vela está apagada
            if (!isLit)
            {
                MostrarTexto();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            OcultarTexto(); // Borra el mensaje al alejarse
        }
    }

    private void MostrarTexto()
    {
        // Evitamos encender la interfaz si la vela de hecho ya está prendida
        if (textoInteraccionUI != null && !isLit)
        {
            textoInteraccionUI.text = "Encender Vela";
            textoInteraccionUI.gameObject.SetActive(true); // Encendemos la UI global
        }
    }

    private void OcultarTexto()
    {
        // Solo intentamos apagarla si nosotros la tenemos asignada de forma segura
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.gameObject.SetActive(false); // Apagamos la UI global
        }
    }
}