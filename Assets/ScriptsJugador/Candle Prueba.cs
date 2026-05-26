using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal; // 🔥 NECESARIO para controlar Light 2D

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
    }

    public void Interact()
    {
        if (!isLit)
        {
            isCorrupted = false;
            LightCandle();
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
}