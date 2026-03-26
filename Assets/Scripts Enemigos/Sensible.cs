using UnityEngine;
using UnityEngine.AI;

public class Sensible : MonoBehaviour, IStimulusReceiver
{
    private enum State { Wandering, Investigating }
    [SerializeField] private State currentState = State.Wandering;

    private NavMeshAgent agent;

    [Header("Patrol Settings")]
    [SerializeField] private float wanderRadius = 7f;
    [SerializeField] private float waitTimeAtPoint = 2f;
    private float wanderTimer;

    [Header("Stimulus Settings")]
    [SerializeField] private float investigationDuration = 3f;
    private float investigationTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    private void Update()
    {
        switch (currentState)
        {
            case State.Wandering:
                HandleWandering();
                break;
            case State.Investigating:
                HandleInvestigation();
                break;
        }
    }

    // --- LÓGICA DE PATRULLA ALEATORIA ---
    private void HandleWandering()
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

    private void SetRandomDestination()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        Vector3 targetPos = transform.position + new Vector3(randomDir.x, randomDir.y, 0);

        // Verificamos que el punto esté dentro del NavMesh
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }

    // --- LÓGICA DE ESTÍMULO ---
    public void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        Debug.Log($"<color=yellow>¡Estímulo {type} detectado!</color> Yendo a {position}");

        currentState = State.Investigating;
        investigationTimer = investigationDuration;
        agent.SetDestination(position);
    }

    private void HandleInvestigation()
    {
        // Si ya llegó al punto del estímulo
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            investigationTimer -= Time.deltaTime;

            if (investigationTimer <= 0)
            {
                Debug.Log("Investigación terminada. Volviendo a patrulla.");
                currentState = State.Wandering;
            }
        }
    }
}