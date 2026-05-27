using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public abstract class EnemyBase : MonoBehaviour, IStimulusReceiver
{
    // Componente de navegación compartido por todos los enemigos del juego.
    protected NavMeshAgent agent;

    // Estados básicos de la máquina de estados de nuestra Inteligencia Artificial.
    protected enum State { Wandering, Investigating, Chasing }
    [SerializeField] protected State currentState = State.Wandering;

    [Header("Base Settings")]
    [SerializeField] protected float attackDistance = 0.8f;
    [SerializeField] protected int attackDamage = 1;
    [SerializeField] private float wanderRadius = 7f; // Radio máximo para buscar puntos de patrulla aleatorios.
    [SerializeField] protected float waitTimeAtPoint = 2f; // Tiempo de espera al llegar a un punto de patrulla.
    [SerializeField] protected LayerMask playerLayer; // Capa física de colisión utilizada para detectar al jugador de forma óptima.

    private float wanderTimer;

    [Header("Stimulus Settings")]
    [SerializeField] protected float investigationDuration = 3f; // Tiempo que se queda revisando la zona de un estímulo.
    protected float investigationTimer;

    protected virtual void Awake()
    {
        // Inicialización y configuración del NavMeshAgent para trabajar correctamente en un entorno 2D plano (desactivando rotaciones tridimensionales).
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        wanderTimer = waitTimeAtPoint;
    }

    protected void MoveTo(Vector2 destination)
    {
        // Verificación de seguridad indispensable para evitar caídas del sistema de navegación de Unity si el enemigo es empujado fuera de la malla.
        if (agent.isOnNavMesh) agent.SetDestination(destination);
    }

    protected virtual void OnEnable()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // Reinicio automático de estados internos, vital para cuando el LevelManager activa y desactiva enemigos entre fases de día y noche.
        currentState = State.Wandering;
        investigationTimer = 0;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        // Restauración visual del SpriteRenderer al reactivar al enemigo (útil si fue afectado por efectos de transparencia).
        if (TryGetComponent(out SpriteRenderer sr))
        {
            sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 1f);
        }
    }

    public virtual void Die()
    {
        PlayDeathEffect();

        // Detiene el movimiento físico pero no destruye el componente para evitar problemas de rendimiento en memoria, preparando el objeto para ser reutilizado.
        if (agent != null) agent.isStopped = true;
        gameObject.SetActive(false);
    }

    protected virtual void CheckAttack()
    {
        // Conexión con el Singleton del jugador para calcular distancias de combate de forma directa.
        if (PlayerController.Instance == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);

        if (!agent.pathPending && distanceToPlayer <= attackDistance)
        {
            PerformAttack();
        }
    }

    protected virtual void PerformAttack()
    {
        // Detección física mediante un círculo de colisión en 2D optimizado utilizando la capa del jugador.
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        // Interconexión con la interfaz IDamageable para aplicar daño a cualquier entidad compatible sin acoplar código.
        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log($"<color=red>JUGADOR DAÑADO por {gameObject.name}</color>");

            // Comportamiento específico: Ciertos enemigos básicos se destruyen inmediatamente tras lograr un impacto exitoso.
            gameObject.SetActive(false);
        }
    }

    protected virtual void HandleWandering()
    {
        // Comprobación del NavMesh para saber si el enemigo llegó a su destino actual de patrulla de forma segura.
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
        // Cálculo matemático de un punto aleatorio dentro de una circunferencia para simular una patrulla orgánica.
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        Vector3 targetPos = transform.position + new Vector3(randomDir.x, randomDir.y, 0);

        // Validación obligatoria en Unity para proyectar el punto aleatorio calculado sobre la superficie real transitable del NavMesh.
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    public virtual void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // Implementación de la interfaz IStimulusReceiver. Conecta el sistema de alertas del mapa (ruido, linternas) con la IA enemiga.
        Debug.Log($"<color=yellow>¡Estímulo {type} detectado por {gameObject.name}!</color>");

        currentState = State.Investigating;
        investigationTimer = investigationDuration;
        agent.SetDestination(position);
    }

    public virtual void GetRepelled(Vector2 shockwaveSource, float force)
    {
        Debug.Log($"{gameObject.name} ha sido repelido por una onda sagrada.");

        // Cálculo vectorial de la dirección de empuje inverso basándose en el origen del impacto (ej. una habilidad del jugador).
        Vector2 pushDirection = ((Vector2)transform.position - shockwaveSource).normalized;

        // Desactivación temporal del agente de navegación para evitar conflictos lógicos mientras se aplica la fuerza física del empuje de forma manual.
        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = pushDirection * force;
        }

        currentState = State.Wandering;

        // Sistema de retraso controlado para devolverle la autonomía a la IA de forma automática tras el impacto.
        Invoke("RecoverFromPush", 1.5f);
    }

    protected virtual void RecoverFromPush()
    {
        if (agent != null)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    protected void PlayDeathEffect()
    {
        // Contenedor modular de efectos visuales. Actualmente actúa como marcador de posición para la futura integración del sistema de partículas de ceniza.
        Debug.Log("Partículas de ceniza apareciendo...");
    }
}