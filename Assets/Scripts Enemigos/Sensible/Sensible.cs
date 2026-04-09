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
           
            PlayerHide playerHide = PlayerController.Instance.GetComponent<PlayerHide>();

            if (playerHide != null && playerHide.IsHidden)
            {
                Debug.Log("El jugador se escondió. Sensible se rinde y vuelve a patrullar.");
                currentState = State.Wandering; // Lo regresamos a patrullar
                return; 
            }

            
            MoveTo(PlayerController.Instance.transform.position);

            
            CheckAttack();
        }
    }

    private void CheckAttack()
    {
        
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
        currentState = State.Wandering; 
    }
    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // Si el estímulo es de un objeto corruptible, el Sensible se hace el sordo y lo ignora
        if (type == StimulusType.Corruptible)
        {
            return;
        }

        // Si es cualquier otro estímulo (ruido del jugador, pasos, etc.), hace el comportamiento normal (Investigar)
        base.OnStimulusReceived(position, type);
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}