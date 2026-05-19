using System.Collections.Generic;
using UnityEngine;

public class CorruptorEnemy : EnemyBase, IStimulusReceiver
{
    [Header("Corruption Logic")]
    [Range(0, 100)]
    [SerializeField] private float successChance = 50f; // 50% de probabilidad por defecto
    [SerializeField] public float waitBeforeAttempt = 4f; // Tiempo que "tarda" en corromper

    [Header("Scanning Settings")]
    [SerializeField] private float scanInterval = 2f; // Tiempo entre escaneos
    private float scanTimer; // El contador interno

    [Header("Memory")]
    private ICorruptible lastAttemptedObject; // Guardamos el último objeto que intentamos corromper

    [Header("Memory System")]
    [SerializeField] private int memoryCapacity = 2; // Cuántos objetos recuerda antes de olvidar el primero
    private List<ICorruptible> memoryList = new List<ICorruptible>(); // Lista de objetos recientes

    private bool isAttempting = false;



    // 1. Añadimos una variable para guardar la posición exacta del objetivo detectado
    private Vector2 currentTargetPosition;

    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        if (type != StimulusType.Corruptible) return;

        // 🔥 Aseguramos que la posición no sea 0,0 si el estímulo trae datos
        currentTargetPosition = position;

        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit.TryGetComponent(out ICorruptible target))
        {
            if (memoryList.Contains(target)) return;
        }

        base.OnStimulusReceived(position, type);
    }


    private void Update()
    {
        if (isAttempting) return;

        if (currentState == State.Wandering)
        {
            HandleWandering();

            scanTimer -= Time.deltaTime;
            if (scanTimer <= 0)
            {
                PassiveScan();
                scanTimer = scanInterval;
            }
        }
        else if (currentState == State.Investigating)
        {
            // 🔥 MEJORA: Verificamos si llegamos al destino
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                Debug.Log("<color=orange>Corruptor: ¡He llegado al objetivo! Iniciando proceso...</color>");
                isAttempting = true;
                StartCoroutine(CorruptionProcess());
            }
        }
    }


    // 2. Ajustamos la detección en la corrutina
    private System.Collections.IEnumerator CorruptionProcess()
    {
        agent.isStopped = true;
        ICorruptible target = null;
        GameObject victimObj = null;

        // Buscamos en un radio pequeño alrededor de donde el enemigo se detuvo
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.5f);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out ICorruptible found))
            {
                // Verificamos que no sea él mismo y que no esté en memoria (opcional)
                if (hit.gameObject != this.gameObject)
                {
                    target = found;
                    victimObj = hit.gameObject;
                    break;
                }
            }
        }

        if (target != null)
        {
            Debug.Log($"<color=yellow>Saboteando: {victimObj.name}...</color>");
            AddToMemory(target);
        }
        else
        {
            Debug.LogWarning($"<color=red>Fallo total: No encontré ICorruptible cerca de {transform.position}</color>");
            FinishAction();
            yield break;
        }

        yield return new WaitForSeconds(waitBeforeAttempt);

        if (Random.Range(0f, 100f) <= successChance)
        {
            target.Corrupt();
            Debug.Log("<color=purple>¡SABOTAJE EXITOSO!</color>");
        }

        FinishAction();
    }

    /*private void ExecuteCorruption()
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
    }*/

    private void FinishAction()
    {
        isAttempting = false;
        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        currentState = State.Wandering;

        // Usamos la función de huida que planeamos antes
        ForceNewWanderPoint();
    }

    private void ForceNewWanderPoint()
    {
        if (currentState == State.Wandering)
        {
            HandleWandering(); // Obligamos a elegir un destino de patrulla
        }
    }
    // --- MODIFICACIÓN EN PASSIVE SCAN ---
    private void PassiveScan()
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(transform.position, 15f);

        foreach (var obj in objects)
        {
            if (obj.TryGetComponent(out ICorruptible target))
            {
                if (memoryList.Contains(target)) continue;

                if (obj.TryGetComponent(out Candle vela))
                {
                    if (!vela.IsLit || vela.IsCorrupted) continue;
                }

                // 🔥 FIJAR POSICIÓN ANTES DE IR
                currentTargetPosition = obj.transform.position;
                Debug.Log($"<color=green>Radar: Objetivo {obj.name} en {currentTargetPosition}</color>");

                OnStimulusReceived(currentTargetPosition, StimulusType.Corruptible);
                return;
            }
        }
    }

    // --- NUEVA LÓGICA DE MEMORIA ---
    private void AddToMemory(ICorruptible newObject)
    {
        // Si el objeto ya está (por seguridad), no lo duplicamos
        if (memoryList.Contains(newObject)) return;

        // Añadimos el nuevo objeto al historial
        memoryList.Add(newObject);

        // Si superamos la capacidad (2 objetos), olvidamos el más antiguo (el índice 0)
        if (memoryList.Count > memoryCapacity)
        {
            Debug.Log("<color=white>Corruptor ha olvidado un objeto antiguo y puede volver a visitarlo.</color>");
            memoryList.RemoveAt(0);
        }
    }
    // Dentro de CorruptorEnemy.cs

    /*public override void GetRepelled(Vector2 shockwaveSource, float force)
    {
        // El Corruptor es más débil a la luz sagrada/onda
        Debug.Log("<color=purple>El Corruptor se disuelve ante la onda...</color>");

        // Llamamos al método Die que ya configuramos en la base
        Die();
    }*/
}