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

    protected virtual void CheckAttack()
    {
        if (PlayerController.Instance == null) return;

        
        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        if (!agent.pathPending && distanceToPlayer <= attackDistance)
        {
            PerformAttack();
        }
    }

    
    protected virtual void PerformAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log($"<color=red>JUGADOR DAÑADO por {gameObject.name}</color>");

            gameObject.SetActive(false); 
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

    public virtual void GetRepelled(Vector2 shockwaveSource, float force)
    {
        Debug.Log($"{gameObject.name} ha sido repelido por una onda sagrada.");

        // 1. Calculamos la dirección opuesta a la onda
        Vector2 pushDirection = ((Vector2)transform.position - shockwaveSource).normalized;

        // 2. Paramos al agente para que no intente luchar contra el empuje
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = pushDirection * force; // Aplicamos un impulso físico inicial
        }

        // 3. Cambiamos el estado a Wandering para que "olvide" su persecución actual
        currentState = State.Wandering;

        // 4. Hacemos que recupere el control después de un segundo
        Invoke("RecoverFromPush", 1.5f);
    }

    protected virtual void RecoverFromPush()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath(); // Obligamos a recalcular ruta
        }
    }

    public virtual void Die()
    {
        Debug.Log($"{gameObject.name} ha sido desterrado.");

        // Aquí disparamos el efecto visual de "muerte"
        // Si tienes un script que maneje partículas, lo llamamos aquí
        PlayDeathEffect();

        // En lugar de destruir el objeto (que causa lag), lo desactivamos
        gameObject.SetActive(false);
    }

    protected void PlayDeathEffect()
    {
        // Buscamos un manejador de efectos en la escena o usamos uno local
        // Por ahora, un simple log, pero aquí irá la lógica de partículas
        Debug.Log("Partículas de ceniza apareciendo...");
    }
}