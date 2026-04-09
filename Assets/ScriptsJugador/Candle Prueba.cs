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

    private bool isLit = false;
    private bool isCorrupted = false;
    private Altar altar;

    public bool IsCorrupted => isCorrupted;
    public bool IsLit => isLit;

    private IEnumerator Start()
    {
        altar = Object.FindFirstObjectByType<Altar>();

        // Esperamos un frame para que los enemigos estén listos
        yield return null;

        if (startLit)
        {
            LightCandle();
        }
    }

    public void Interact()
    {
        if (isCorrupted)
        {
            Debug.Log("La vela está impregnada de malicia.");
            return;
        }
        if (!isLit)
        {
            LightCandle();
        }
    }

    private void LightCandle()
    {
        isLit = true;
        Debug.Log($"{gameObject.name}: Encendida");

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
        Debug.Log("<color=purple>Vela Corrompida</color>");

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
        if (altar != null) altar.UpdateAltarHealth();
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
}