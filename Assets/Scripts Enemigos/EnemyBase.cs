using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour, IStimulusReceiver
{
    protected NavMeshAgent agent;
    protected Transform player;
    protected enum State { Wandering, Investigating, Chasing }
    [SerializeField] protected State currentState = State.Wandering;
    [Header("Base Settings")]
    [SerializeField] protected float attackDistance = 0.8f;
    [SerializeField] private float wanderRadius = 7f;
    [SerializeField] private float waitTimeAtPoint = 2f;

    private float wanderTimer;

    [Header("Stimulus Settings")]
    [SerializeField] protected float investigationDuration = 3f;
    protected float investigationTimer;

    protected virtual void Awake()
    {

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        wanderTimer = waitTimeAtPoint;
    }

    protected void MoveTo(Vector2 destination)
    {
        if (agent.isOnNavMesh) agent.SetDestination(destination);
    }

    protected virtual void CheckAttack()
    {
        if (player != null && !agent.pathPending && agent.remainingDistance < attackDistance)
        {
            PerformAttack();
        }
    }

    protected virtual void PerformAttack()
    {
        Debug.Log("<color=red>JUGADOR DAÑADO por " + gameObject.name + "</color>");
        gameObject.SetActive(false); // El NPC desaparece tras el golpe
    }
    protected virtual void HandleWandering()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            wanderTimer -= Time.deltaTime;
            if (wanderTimer <= 0)
            {
                SetRandomDestination();
                wanderTimer = waitTimeAtPoint;
            }
        }
    }
    protected virtual void SetRandomDestination()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        Vector3 targetPos = transform.position + new Vector3(randomDir.x, randomDir.y, 0);

        // Verificamos que el punto esté dentro del NavMesh
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }
    public virtual void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // Si soy un Corruptor y el estímulo NO es Corruptible, lo ignoro
        if (this is CorruptorEnemy && type != StimulusType.Corruptible) return;

        // Si soy un Sensible y el estímulo es Corruptible, lo ignoro (opcional)
        // if (this is SensibleEnemy && type == StimulusType.Corruptible) return;

        Debug.Log($"<color=yellow>¡Estímulo {type} detectado por {gameObject.name}!</color>");

        currentState = State.Investigating;
        investigationTimer = investigationDuration;
        agent.SetDestination(position);
    }
}