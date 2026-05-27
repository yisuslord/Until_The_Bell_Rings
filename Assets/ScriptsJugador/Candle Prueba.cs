using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using TMPro;

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
    [SerializeField] private float lightIntensity = 1.0f;

    [Header("UI de Interaccion (Auto-asignada)")]
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

        // Resolucion automatica de la dependencia lumínica en la estructura de hijos si no fue asignada en inspector
        if (candleLight == null) candleLight = GetComponentInChildren<Light2D>();

        // Inicializacion del estado logico base previo al procesamiento del ciclo de Start
        if (startLit)
        {
            isLit = true;
        }

        // Busqueda exhaustiva en buffer para localizar el elemento de interfaz sin importar su estado de activacion jerarquica
        TextMeshProUGUI[] todosLosTextos = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();

        foreach (var texto in todosLosTextos)
        {
            if (texto.gameObject.name == "TextoInteraccion")
            {
                textoInteraccionUI = texto;
                break;
            }
        }

        // Inicializacion de seguridad del buffer de texto
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.text = "";
        }
        else
        {
            Debug.LogWarning($"[Candle] No se encontro el GameObject 'TextoInteraccion' en la escena para {gameObject.name}");
        }
    }

    private void Start()
    {
        altar = Object.FindFirstObjectByType<Altar>();

        // Sincronizacion de componentes graficos y lúmenes en base al estado definido en Awake
        UpdateVisuals();

        if (isLit)
        {
            // Propagacion de estimulos y notificacion a los sistemas globales si el item inicia encendido
            NotifyAltar();
            EmitStimulus(StimulusType.Light);
            EmitStimulus(StimulusType.Corruptible);
            Debug.Log($"[Mecanica] {gameObject.name}: Inicializada en estado activo (Con luz real).");
        }
        else
        {
            OcultarTexto();
        }
    }

    // Procesa el intento de interaccion del jugador con el objeto de entorno
    public void Interact()
    {
        // Restriccion de ejecucion: Solo procesa la accion si el estado actual es inactivo
        if (!isLit)
        {
            isCorrupted = false;
            LightCandle();
            OcultarTexto();
        }
    }

    private void LightCandle()
    {
        isLit = true;
        Debug.Log($"[Mecanica] {gameObject.name}: Cambio de estado a Activo.");

        UpdateVisuals();
        NotifyAltar();

        EmitStimulus(StimulusType.Light);
        EmitStimulus(StimulusType.Corruptible);
    }

    // Ejecuta la interrupcion del estado activo debido a agentes externos de corrupcion
    public void Corrupt()
    {
        if (isCorrupted) return;

        isCorrupted = true;
        isLit = false;
        Debug.Log("<color=purple>[Sistema] Vela apagada y componente de iluminacion desactivado por Corruptor.</color>");

        UpdateVisuals();
        NotifyAltar();

        // Actualizacion reactiva del prompt de interfaz si el usuario se encuentra dentro del rango de interaccion
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

    // Transmite una llamada radial de estimulos a los receptores dentro de la capa especificada
    private void EmitStimulus(StimulusType type)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, lightRadius, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
                receiver.OnStimulusReceived(transform.position, type);
        }
    }

    // Sincroniza los estados logicos booleanos con las propiedades de los componentes de renderizado y luces de Universal RP
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

    // --- DETECCION EN RANGO PARA LA INTERFAZ DE USUARIO ---

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;

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
            OcultarTexto();
        }
    }

    private void MostrarTexto()
    {
        if (textoInteraccionUI != null && !isLit)
        {
            textoInteraccionUI.text = "Encender Vela";
            textoInteraccionUI.gameObject.SetActive(true);
        }
    }

    private void OcultarTexto()
    {
        if (textoInteraccionUI != null)
        {
            textoInteraccionUI.gameObject.SetActive(false);
        }
    }
}