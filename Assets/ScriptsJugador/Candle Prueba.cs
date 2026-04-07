using UnityEngine;
using System.Collections;


public class Candle : MonoBehaviour, IInteractable, ICorruptible
{
    private bool isLit = false;
    private bool isCorrupted = false; // Implementación de ICorruptible
    [SerializeField] private float lightRadius = 8f;
    [SerializeField] private LayerMask enemyLayer; // Configura esto a la capa "Enemies"
    [SerializeField] private float corruptionDuration = 60f; // 1 minuto por defecto
    public bool IsCorrupted => isCorrupted;

    public void Interact()
    {
        if (isCorrupted)
        {
            Debug.Log("La vela está impregnada de malicia y no puede encenderse.");
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
        Debug.Log("Vela Encendida");
        EmitStimulus(StimulusType.Light);
        // También emitimos un estímulo para que el Corruptor sepa que esta vela es un objetivo
        EmitStimulus(StimulusType.Corruptible);
    }

    public void Corrupt()
    {
        if (isCorrupted) return;

        isCorrupted = true;
        isLit = false; // Apaga la vela
        Debug.Log("<color=purple>Vela Corrompida por el NPC</color>");

        // Podrías cambiar el color del sprite aquí a morado
        StartCoroutine(RestoreTimer());
    }

    public void Restore()
    {
        isCorrupted = false;
        Debug.Log("La vela ya no está corrompida.");
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