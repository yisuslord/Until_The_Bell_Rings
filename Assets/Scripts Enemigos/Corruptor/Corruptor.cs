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
    [SerializeField] private AudioClip clipCorromper; // Sonido al sabotear con éxito
    [SerializeField] private AudioClip clipBusqueda;  // 🔥 Sonido cuando el radar localiza una vela válida

    private Vector2 currentTargetPosition;

    // 🔥 CANDADO DE AUDIO: Evita que el sonido de búsqueda se solape si encuentra estímulos seguidos
    private bool yaSonoBusqueda = false;

    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        if (type != StimulusType.Corruptible) return;

        Collider2D hit = Physics2D.OverlapPoint(position);
        if (hit != null && hit.TryGetComponent(out ICorruptible target))
        {
            if (memoryList.Contains(target))
            {
                return; // 🛑 CANDADO 1: Rechazado si ya lo recuerda
            }
        }

        currentTargetPosition = position;
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
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
            {
                Debug.Log("<color=orange>Corruptor: ¡He llegado al objetivo! Iniciando proceso...</color>");
                isAttempting = true;
                StartCoroutine(CorruptionProcess());
            }
        }
    }

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

        // Espera los segundos de canalización/animación del sabotaje
        yield return new WaitForSeconds(waitBeforeAttempt);

        // Tirada de dados para el éxito de la corrupción
        if (Random.Range(0f, 100f) <= successChance)
        {
            // 🔥 IMPACTO ÚNICO DE SABOTAJE: Suena en el frame exacto del éxito
            if (AudioManager.Instance != null && clipCorromper != null)
            {
                AudioManager.Instance.PlaySFX2D(clipCorromper, 0.4f); // Volumen calibrado para dar un buen susto ambiental
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

    private void FinishAction()
    {
        isAttempting = false;
        yaSonoBusqueda = false; // 🔄 RESETEO DEL RADAR: Al terminar una acción, su radar vuelve a estar listo para pitar

        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        currentState = State.Wandering;
        ForceNewWanderPoint();
    }

    private void ForceNewWanderPoint()
    {
        if (currentState == State.Wandering)
        {
            HandleWandering();
        }
    }

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

                // 🔥 CONEXIÓN DEL AUDIO DE BÚSQUEDA:
                // Suena una sola vez en el instante donde el radar bloquea una vela encendida para perseguirla
                if (!yaSonoBusqueda)
                {
                    if (AudioManager.Instance != null && clipBusqueda != null)
                    {
                        AudioManager.Instance.PlaySFX2D(clipBusqueda, 0.25f); // Un sonido sutil (un click mecánico o un pulso)
                    }
                    yaSonoBusqueda = true;
                }

                OnStimulusReceived(currentTargetPosition, StimulusType.Corruptible);
                return;
            }
        }
    }

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