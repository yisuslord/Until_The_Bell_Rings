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

    // 🔥 NUEVO: Referencia al componente de luz real
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
    }

    private IEnumerator Start()
    {
        altar = Object.FindFirstObjectByType<Altar>();

        // Estado inicial
        UpdateVisuals();

        yield return null;

        if (startLit)
        {
            LightCandle();
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
        // Si queremos que al restaurarse siga apagada hasta que el player la toque,
        // solo actualizamos visuales. Si queremos que se prenda sola, llamamos a LightCandle().
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

    // 🔥 ACTUALIZADO: Maneja el color del Sprite Y el estado de la Luz 2D
    private void UpdateVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isLit ? litColor : unlitColor;
        }

        if (candleLight != null)
        {
            // La luz se activa solo si está encendida
            candleLight.enabled = isLit;
            candleLight.intensity = isLit ? lightIntensity : 0f;

            // Opcional: Si está corrompida, podrías poner la luz morada en vez de apagarla
            /*
            if(isCorrupted) {
                candleLight.enabled = true;
                candleLight.color = Color.magenta;
            }
            */
        }
    }
}