using UnityEngine;

public class CorruptorEnemy : EnemyBase, IStimulusReceiver
{
    [Header("Corruption Logic")]
    [Range(0, 100)]
    [SerializeField] private float successChance = 50f; // 50% de probabilidad por defecto
    [SerializeField] private float waitBeforeAttempt = 4f; // Tiempo que "tarda" en corromper
    

    private bool isAttempting = false;




    private void Update()
    {
        // Solo si no estamos intentando corromper, procesamos los estados normales
        if (!isAttempting)
        {
            if (currentState == State.Wandering)
            {
                HandleWandering();
            }
            else if (currentState == State.Investigating)
            {
                // Comprobamos si ya llegamos al punto del estímulo
                if (!agent.pathPending && agent.remainingDistance < 0.7f && agent.hasPath)
                {
                    StartCoroutine(CorruptionProcess());
                }
            }
        }
    }


    private System.Collections.IEnumerator CorruptionProcess()
    {
        isAttempting = true;
        agent.isStopped = true;
        // Cambiamos el estado a Investigating explícitamente para que nada más lo mueva
        currentState = State.Investigating;

        Debug.Log("Iniciando espera de " + waitBeforeAttempt + " segundos...");

        yield return new WaitForSeconds(waitBeforeAttempt);

        // Si llegamos aquí, la corrutina sigue viva
        Debug.Log("Espera terminada. Calculando probabilidad...");

        float roll = Random.Range(0f, 100f);
        Debug.Log($"Roll: {roll} / SuccessChance: {successChance}");

        if (roll <= successChance)
        {
            ExecuteCorruption();
            Debug.Log("kjdckj");
        }
        else
        {
            Debug.Log("<color=cyan>¡El Corruptor falló en su intento!</color>");
            FinishAction();
        }
    }

    private void ExecuteCorruption()
    {
        // Aumentamos un poco el radio para ser más generosos
        float scanRadius = 1.5f;

        // Escaneamos TODO en ese radio para debuguear
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius);

        Debug.Log($"ExecuteCorruption: Se encontraron {hits.Length} colisionadores cerca.");

        bool foundTarget = false;
        foreach (var hit in hits)
        {
            // Ignoramos si el colisionador es el del propio NPC
            if (hit.gameObject == gameObject) continue;

            Debug.Log($"Analizando objeto: {hit.name}");

            if (hit.TryGetComponent(out ICorruptible target))
            {
                target.Corrupt();
                Debug.Log("<color=purple>¡SABOTAJE EXITOSO en " + hit.name + "!</color>");
                foundTarget = true;
                break; // Ya encontramos el objetivo, no hace falta seguir
            }
        }

        if (!foundTarget)
        {
            Debug.LogWarning("<color=orange>ExecuteCorruption: No se encontró ningún objeto con ICorruptible cerca.</color>");
        }

        FinishAction();
    }

    private void FinishAction()
    {
        isAttempting = false;
        agent.isStopped = false;
        // Aquí decides: ¿Desaparece tras un intento o vuelve a patrullar?
        // De momento lo desactivamos para que no lo intente infinitamente en el mismo frame
        currentState = State.Wandering;
        //gameObject.SetActive(false);
    }
}