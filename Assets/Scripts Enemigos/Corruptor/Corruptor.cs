using UnityEngine;

public class CorruptorEnemy : EnemyBase, IStimulusReceiver
{
    [Header("Corruption Logic")]
    [Range(0, 100)]
    [SerializeField] private float successChance = 50f; // 50% de probabilidad por defecto
    [SerializeField] private float waitBeforeAttempt = 4f; // Tiempo que "tarda" en corromper

    [Header("Scanning Settings")]
    [SerializeField] private float scanInterval = 2f; // Tiempo entre escaneos
    private float scanTimer; // El contador interno

    [Header("Memory")]
    private ICorruptible lastAttemptedObject; // Guardamos el último objeto que intentamos corromper


    private bool isAttempting = false;



    // Dentro de CorruptorEnemy.cs
    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // 1. Si no es corruptible, ignorar.
        if (type != StimulusType.Corruptible) return;

        // 2. FILTRO DE MEMORIA: Buscamos qué objeto hay en esa posición
        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit.TryGetComponent(out ICorruptible target))
        {
            // Si el objeto en esa posición es el que acabamos de fallar, BLOQUEAMOS el estímulo
            if (target == lastAttemptedObject)
            {
                // Debug.Log("Ignorando estímulo de vela fallida recientemente.");
                return;
            }
        }

        // 3. Solo si pasó los filtros, dejamos que la base lo mande a investigar
        base.OnStimulusReceived(position, type);
    }


    private void Update()
    {
        if (isAttempting) return; // Si está trabajando, no hace nada más

        if (currentState == State.Wandering)
        {
            HandleWandering();

            // Escaneo de "radar" constante
            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0)
            {
                PassiveScan();
                scanTimer = scanInterval;
            }
        }
        else if (currentState == State.Investigating)
        {
            // Si ya llegó a la vela...
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.3f)
            {
                // Iniciamos el proceso y nos aseguramos de no entrar aquí dos veces
                isAttempting = true;
                StartCoroutine(CorruptionProcess());
            }
        }
    }


    private System.Collections.IEnumerator CorruptionProcess()
    {
        isAttempting = true;
        agent.isStopped = true;

        // Detectamos qué objeto tenemos enfrente justo ahora para guardarlo en memoria
        Collider2D hit = Physics2D.OverlapCircle(transform.position, 1.5f);
        if (hit != null && hit.TryGetComponent(out ICorruptible target))
        {
            lastAttemptedObject = target; // Guardamos la interfaz, no el collider
        }

        Debug.Log("Iniciando intento de sabotaje...");
        yield return new WaitForSeconds(waitBeforeAttempt);

        float roll = Random.Range(0f, 100f);

        if (roll <= successChance)
        {
            ExecuteCorruption();
            // Si tiene éxito, podemos limpiar la memoria porque el objeto ya está desactivado
            lastAttemptedObject = null;
        }
        else
        {
            Debug.Log("<color=cyan>¡Falló! Recordaré este objeto para no repetir de inmediato.</color>");
            FinishAction();
        }
    }

    private void ExecuteCorruption()
    {
        
        float scanRadius = 1.5f;

       
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, scanRadius);

        Debug.Log($"ExecuteCorruption: Se encontraron {hits.Length} colisionadores cerca.");

        bool foundTarget = false;
        foreach (var hit in hits)
        {
           
            if (hit.gameObject == gameObject) continue;

            Debug.Log($"Analizando objeto: {hit.name}");

            if (hit.TryGetComponent(out ICorruptible target))
            {
                target.Corrupt();
                Debug.Log("<color=purple>¡SABOTAJE EXITOSO en " + hit.name + "!</color>");
                foundTarget = true;
                break; 
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

        // 1. FUNDAMENTAL: Borramos la ruta actual. 
        // Si no hacemos esto, el agente cree que su destino sigue siendo la vela donde está parado.
        agent.ResetPath();

        // 2. Volvemos al estado inicial
        currentState = State.Wandering;

        Debug.Log("<color=white>Acción terminada. Volviendo a patrullar...</color>");

        // 3. Le damos un pequeño empujón para que no se quede quieto
        // Esto lo obliga a buscar un nuevo punto de patrulla inmediatamente
        Invoke("ForceNewWanderPoint", 0.1f);
    }

    private void ForceNewWanderPoint()
    {
        if (currentState == State.Wandering)
        {
            HandleWandering(); // Obligamos a elegir un destino de patrulla
        }
    }
    private void PassiveScan()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, 15f);

        foreach (var obj in objects)
        {
            if (obj.TryGetComponent(out ICorruptible corruptibleTarget))
            {
                // Verificamos si es una vela para ver si está encendida
                if (obj.TryGetComponent(out Candle vela))
                {
                    // REGLA DE ORO: 
                    // 1. Debe estar encendida.
                    // 2. No debe estar corrompida.
                    // 3. NO DEBE SER LA MISMA QUE ACABAMOS DE INTENTAR (si fallamos).
                    if (vela.IsLit && !vela.IsCorrupted && corruptibleTarget != lastAttemptedObject)
                    {
                        Debug.Log($"¡Vela nueva detectada! Yendo a {obj.name}");
                        OnStimulusReceived(obj.transform.position, StimulusType.Corruptible);
                        return;
                    }
                }
            }
        }
    }

    // Dentro de CorruptorEnemy.cs

    public override void GetRepelled(Vector2 shockwaveSource, float force)
    {
        // El Corruptor es más débil a la luz sagrada/onda
        Debug.Log("<color=purple>El Corruptor se disuelve ante la onda...</color>");

        // Llamamos al método Die que ya configuramos en la base
        Die();
    }
}