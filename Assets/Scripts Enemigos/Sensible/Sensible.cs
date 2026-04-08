using UnityEngine;
using UnityEngine.AI;

public class Sensible : EnemyBase
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 4f;

    [Header("Combat Settings")]
    [SerializeField] private float cooldownTime = 3f;
    private bool isStunned = false;

    private void Update()
    {
        if (isStunned) return;

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

    private void HandleChasing()
    {
        if (PlayerController.Instance != null)
        {
            // 🔥 LA SOLUCIÓN: Revisamos si el jugador tiene el script y está escondido
            PlayerHide playerHide = PlayerController.Instance.GetComponent<PlayerHide>();

            if (playerHide != null && playerHide.IsHidden)
            {
                Debug.Log("El jugador se escondió. Sensible se rinde y vuelve a patrullar.");
                currentState = State.Wandering; // Lo regresamos a patrullar
                return; // Cortamos aquí para que no ejecute el MoveTo ni el CheckAttack
            }

            // 1. Se mueve hacia el jugador tranquilamente (pueden superponerse)
            MoveTo(PlayerController.Instance.transform.position);

            // 2. En cada frame, lanza el radar para ver si ya lo está tocando
            CheckAttack();
        }
    }

    private void CheckAttack()
    {
        // El OverlapCircle funciona perfecto aunque la matriz de colisiones esté apagada
        // Solo necesita que la variable 'playerLayer' esté bien asignada en el inspector
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log("<color=red>¡Sensible golpeó al jugador!</color>");

            StartCoroutine(AttackCooldown());
        }
    }

    private System.Collections.IEnumerator AttackCooldown()
    {
        isStunned = true;
        agent.isStopped = true;

        Debug.Log($"Sensible descansando por {cooldownTime} segundos...");
        yield return new WaitForSeconds(cooldownTime);

        isStunned = false;
        agent.isStopped = false;
        currentState = State.Wandering; // Vuelve a patrullar
    }

    private void HandleInvestigation()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            CheckForPlayerProximity();
            investigationTimer -= Time.deltaTime;

            if (investigationTimer <= 0)
            {
                currentState = State.Wandering;
            }
        }
    }

    private void CheckForPlayerProximity()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (hit != null) currentState = State.Chasing;
    }

    // Esto te ayudará a ver la distancia de ataque real en la escena (un círculo rojo)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}