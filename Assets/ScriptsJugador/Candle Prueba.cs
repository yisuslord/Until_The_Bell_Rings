using UnityEngine;
using System.Collections;

public class Candle : MonoBehaviour, IInteractable, ICorruptible
{
    [Header("Initial State")]
    [SerializeField] private bool startLit = false; // El "Switch" en el Inspector

    [Header("Settings")]
    [SerializeField] private float lightRadius = 8f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float corruptionDuration = 60f;

    // 🔥 NUEVO: Configuración visual (Solo Encendido y Apagado)
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color litColor = Color.white; // Color cuando está prendida
    [SerializeField] private Color unlitColor = Color.gray; // Color cuando está apagada

    private bool isLit = false;
    private bool isCorrupted = false;
    private Altar altar;

    public bool IsCorrupted => isCorrupted;
    public bool IsLit => isLit;

    private void Awake()
    {
        // Buscamos el componente automáticamente por si olvidas asignarlo
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private IEnumerator Start()
    {
        altar = Object.FindFirstObjectByType<Altar>();

        // Establecemos el color inicial
        UpdateColor();

        // Esperamos un frame para que los enemigos estén listos
        yield return null;

        if (startLit)
        {
            LightCandle();
        }
    }

    public void Interact()
    {
        // Si está apagada, la prendemos y limpiamos cualquier estado de corrupción
        if (!isLit)
        {
            isCorrupted = false;
            LightCandle();
        }
    }

    private void LightCandle()
    {
        isLit = true;
        Debug.Log($"{gameObject.name}: Encendida");

        UpdateColor(); // 🔥 Cambia a color Encendida
        NotifyAltar();

        // Emitimos estímulos: 
        // 1. Luz para el Sensible.
        // 2. Corruptible para que el Corruptor sepa que puede atacarla.
        EmitStimulus(StimulusType.Light);
        EmitStimulus(StimulusType.Corruptible);
    }

    public void Corrupt()
    {
        if (isCorrupted) return;

        isCorrupted = true;
        isLit = false;
        Debug.Log("<color=purple>Vela apagada por el Corruptor</color>");

        UpdateColor(); // 🔥 Cambia a color Apagada
        NotifyAltar();
        StartCoroutine(RestoreTimer());
    }

    public void Restore()
    {
        isCorrupted = false;
        NotifyAltar();
    }

    private void NotifyAltar()
    {
        if (altar != null) altar.NotifyCandleChanged();
    }

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

    // 🔥 NUEVO: Lógica de colores simplificada
    private void UpdateColor()
    {
        if (spriteRenderer == null) return;

        if (isLit)
        {
            spriteRenderer.color = litColor;
        }
        else
        {
            // Tanto si está apagada normal como si la apagó el Corruptor, usa el mismo color
            spriteRenderer.color = unlitColor;
        }
    }
}