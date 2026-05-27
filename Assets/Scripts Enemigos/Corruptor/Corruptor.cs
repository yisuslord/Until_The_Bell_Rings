using System.Collections.Generic;
using UnityEngine;

public class CorruptorEnemy : EnemyBase, IStimulusReceiver
{
    [Header("Corruption Logic")]
    [Range(0, 100)]
    [SerializeField] public float successChance = 50f;
    [SerializeField] public float waitBeforeAttempt = 4f;

    [Header("Scanning Settings")]
    [SerializeField] public float scanInterval = 2f;
    private float scanTimer;

    [Header("Memory System")]
    [SerializeField] private int memoryCapacity = 2;
    private List<ICorruptible> memoryList = new List<ICorruptible>();

    private bool isAttempting = false;

    [Header("Audio System")]
    [SerializeField] private AudioClip clipCorromper;
    [SerializeField] private AudioClip clipBusqueda;

    private Vector2 currentTargetPosition;

    private bool yaSonoBusqueda = false;

    // Se conecta con el sistema de estimulos global.
    // Revisa si la posicion del estimulo recibido contiene un objeto corruptible para guardarlo en memoria y activar la busqueda base.
    // Esto evita que el enemigo pierda tiempo persiguiendo un objetivo que ya saboteo hace poco.
    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        if (type != StimulusType.Corruptible) return;

        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit.TryGetComponent(out ICorruptible target))
        {
            if (memoryList.Contains(target))
            {
                return;
            }
        }

        currentTargetPosition = position;
        base.OnStimulusReceived(position, type);
    }

    // Controla el bucle de comportamiento del enemigo frame a frame.
    // Si esta de vago, ejecuta su patrullaje y descuenta el temporizador para realizar escaneos de area repetitivos.
    // Si esta investigando, verifica si llego al destino para detenerse e iniciar la corrutina de sabotaje.
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
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                Debug.Log("<color=orange>Corruptor: ¡He llegado al objetivo! Iniciando proceso...</color>");
                isAttempting = true;
                StartCoroutine(CorruptionProcess());
            }
        }
    }

    // Corrutina que maneja la secuencia de interaccion con el objetivo.
    // Se conecta con los componentes fisicos cercanos y con el AudioManager.
    // Detiene al agente, busca un objetivo valido en un radio cercano que no este en su memoria, aplica un tiempo de espera simulando una canalizacion y realiza un calculo probabilistico para determinar si corrompe el objeto o falla, reproduciendo el sonido correspondiente si tiene exito.
    private System.Collections.IEnumerator CorruptionProcess()
    {
        agent.isStopped = true;
        ICorruptible target = null;
        GameObject victimObj = null;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2.5f);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out ICorruptible found))
            {
                if (hit.gameObject != this.gameObject)
                {
                    if (memoryList.Contains(found)) continue;

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
            Debug.LogWarning($"<color=red>Acción abortada: El objeto cercano ya fue saboteado recientemente o no es válido.</color>");
            FinishAction();
            yield break;
        }

        yield return new WaitForSeconds(waitBeforeAttempt);

        if (Random.Range(0f, 100f) <= successChance)
        {
            if (AudioManager.Instance != null && clipCorromper != null)
            {
                AudioManager.Instance.PlaySFX2D(clipCorromper, 0.4f);
            }
            target.Corrupt();
            Debug.Log("<color=purple>¡SABOTAJE EXITOSO!</color>");
        }
        else
        {
            Debug.Log("<color=cyan>El intento de sabotaje falló por probabilidad, pero el enemigo recuerda la vela.</color>");
        }

        FinishAction();
    }

    // Restablece las variables de control de accion y el candado de audio de busqueda.
    // Reactiva el movimiento del componente NavMeshAgent y regresa al estado de patrullaje eligiendo un punto nuevo.
    // Su proposito es limpiar el estado del enemigo para que pueda volver a buscar objetivos de forma limpia sin quedarse trabado.
    private void FinishAction()
    {
        isAttempting = false;
        yaSonoBusqueda = false;

        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        currentState = State.Wandering;
        ForceNewWanderPoint();
    }

    // Fuerza la busqueda inmediata de un nuevo punto de patrullaje.
    // Llama al metodo heredado de la IA de movimiento si se encuentra en el estado correcto.
    private void ForceNewWanderPoint()
    {
        if (currentState == State.Wandering)
        {
            HandleWandering();
        }
    }

    // Realiza un escaneo radial constante para detectar velas en el entorno.
    // Se conecta con el componente Candle de los objetos detectados y con el AudioManager.
    // Filtra las velas que ya esten apagadas, corrompidas o en memoria. Si encuentra una vela encendida valida, reproduce el sonido de alerta una sola vez mediante su candado booleano y manda la posicion al sistema de estimulos para iniciar la persecucion.
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

                currentTargetPosition = obj.transform.position;
                Debug.Log($"<color=green>Radar: Objetivo válido encontrado -> {obj.name}</color>");

                if (!yaSonoBusqueda)
                {
                    if (AudioManager.Instance != null && clipBusqueda != null)
                    {
                        AudioManager.Instance.PlaySFX2D(clipBusqueda, 0.25f);
                    }
                    yaSonoBusqueda = true;
                }

                OnStimulusReceived(currentTargetPosition, StimulusType.Corruptible);
                return;
            }
        }
    }

    // Registra un objeto en la lista de memoria del enemigo.
    // Controla que el tamano de la lista no supere la capacidad maxima establecida. Si se excede, elimina el registro mas antiguo.
    // Esto sirve para que el enemigo no se quede estancado saboteando el mismo objeto infinitamente, permitiendo que eventualmente olvide una vela y pueda volver a atacarla si el jugador la repara.
    private void AddToMemory(ICorruptible newObject)
    {
        if (memoryList.Contains(newObject)) return;

        memoryList.Add(newObject);

        if (memoryList.Count > memoryCapacity)
        {
            Debug.Log("<color=white>Corruptor ha olvidado el objeto más antiguo de su lista.</color>");
            memoryList.RemoveAt(0);
        }
    }
}