using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour, IStimulusReceiver
{
    protected NavMeshAgent agent;
    protected enum State { Wandering, Investigating, Chasing }
    [SerializeField] protected State currentState = State.Wandering;

    [Header("Base Settings")]
    [SerializeField] protected float attackDistance = 0.8f;
    [SerializeField] protected int attackDamage = 1;
    [SerializeField] private float wanderRadius = 7f;
    [SerializeField] private float waitTimeAtPoint = 2f;
    [SerializeField] protected LayerMask playerLayer;

    private float wanderTimer;

    [Header("Stimulus Settings")]
    [SerializeField] protected float investigationDuration = 3f;
    protected float investigationTimer;

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        wanderTimer = waitTimeAtPoint;
    }

    protected void MoveTo(Vector2 destination)
    {
        if (agent.isOnNavMesh) agent.SetDestination(destination);
    }

    // Lo hacemos PROTECTED VIRTUAL para que sus hijos puedan usarlo o cambiarlo.
    // Usamos el Singleton (PlayerController.Instance) para saber dónde está el jugador.
    protected virtual void CheckAttack()
    {
        if (PlayerController.Instance == null) return;

        // Calculamos la distancia real entre este enemigo y el jugador
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        if (!agent.pathPending && distanceToPlayer <= attackDistance)
        {
            PerformAttack();
        }
    }

    // El ataque por defecto: Golpea y desaparece (ideal para el Manifestado/Corruptor)
    // El Sensible NO usará esto, porque él usa su propio Hitbox.
    protected virtual void PerformAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log($"<color=red>JUGADOR DAÑADO por {gameObject.name}</color>");

            gameObject.SetActive(false); // Por defecto, desaparecen al golpear
        }
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

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    public virtual void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        Debug.Log($"<color=yellow>¡Estímulo {type} detectado por {gameObject.name}!</color>");

        currentState = State.Investigating;
        investigationTimer = investigationDuration;
        agent.SetDestination(position);
    }
}