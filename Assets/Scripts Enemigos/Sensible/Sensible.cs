using UnityEngine;
using UnityEngine.AI;

public class Sensible : EnemyBase, IStimulusReceiver
{
    public Transform playertransform;

    [Header("Patrol Settings")]
   

    /*[Header("Stimulus Settings")]
    [SerializeField] private float investigationDuration = 3f;
    private float investigationTimer;*/

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 4f; // Tamaño del círculo de detección
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

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
            case State.Chasing:
                HandleChasing();
                break;
        }
    }

    private void CheckForPlayerProximity()
    {
        // Creamos un círculo invisible que detecta la capa del jugador
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerCollider != null)
        {
            Debug.Log("<color=orange>¡Jugador detectado en el área!</color>");
            currentState = State.Chasing;
        }
    }

    // --- LÓGICA DE PERSECUCIÓN Y ATAQUE ---
    private void HandleChasing()
    {
        

        if (player != null)
        {
            MoveTo(player.position);
            CheckAttack();
        }
    }

    /*private void Attack()
    {
        Debug.Log("<color=red>JUGADOR DAÑADO</color>");

        // Desactivar el NPC como pediste
        gameObject.SetActive(false);
    }*/

    // --- LÓGICA DE PATRULLA ALEATORIA ---
    /*private void HandleWandering()
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
    }*/

    /*private void SetRandomDestination()
    {
        Vector2 randomDir = Random.insideUnitCircle * wanderRadius;
        Vector3 targetPos = transform.position + new Vector3(randomDir.x, randomDir.y, 0);

        // Verificamos que el punto esté dentro del NavMesh
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, wanderRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }*/

    // --- LÓGICA DE ESTÍMULO ---
    /*public void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        if (currentState == State.Chasing) return; // Si ya está persiguiendo, ignora luces

        Debug.Log($"<color=yellow>¡Estímulo {type} detectado!</color> Yendo a {position}");


        currentState = State.Investigating;
        investigationTimer = investigationDuration;
        agent.SetDestination(position);
        
    }*/
    // --- VISUALIZACIÓN EN EL EDITOR ---
    private void OnDrawGizmosSelected()
    {
        // Dibuja el círculo de detección en la ventana Scene para que puedas ajustarlo
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void HandleInvestigation()
    {
        
        // Si ya llegó al punto del estímulo
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            CheckForPlayerProximity();
            investigationTimer -= Time.deltaTime;

            if (investigationTimer <= 0)
            {
                Debug.Log("Investigación terminada. Volviendo a patrulla.");
                currentState = State.Wandering;
            }
        }
    }

    
}