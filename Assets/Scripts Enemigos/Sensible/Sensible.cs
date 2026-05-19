using UnityEngine;
using System.Collections;

public class Sensible : EnemyBase
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private AltarZone altarZone;

    [Header("Combat Settings")]
    [SerializeField] private float cooldownTime = 3f;
    public Animator anim;
    private bool moving;
    private bool isStunned = false;
    private bool isAttacking = false; // Candado para la animación de ataque

    [Header("Audio System")]
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipGolpe;

    private void Update()
    {
        moving = !isStunned && !isAttacking && agent.velocity.magnitude > 0.1f;
        Animate();

        if (isStunned || isAttacking) return;

        if (altarZone != null && altarZone.IsPlayerInside)
        {
            if (currentState != State.Wandering)
            {
                Debug.Log("Jugador a salvo en Altar. Sensible vuelve a patrullar.");
                currentState = State.Wandering;
            }
            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                SetRandomDestination(); // Función heredada de EnemyBase para que camine a otro lado
            }
        }

        switch (currentState)
        {
            case State.Wandering: HandleWandering(); break;
            case State.Investigating: HandleInvestigation(); break;
            case State.Chasing: HandleChasing(); break;
        }
    }

    private void HandleChasing()
    {
        if (AudioManager.Instance != null && clipAtaque != null)
        {
            // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
            AudioManager.Instance.PlaySFX2D(clipAtaque, 1f);
        }
        if (PlayerController.Instance != null)
        {
            if (altarZone.IsPlayerInside) return;

            PlayerHide playerHide = PlayerController.Instance.GetComponent<PlayerHide>();
            if (playerHide != null && playerHide.IsHidden)
            {
                currentState = State.Wandering;
                return;
            }

            MoveTo(PlayerController.Instance.transform.position);
            CheckAttack();
        }
    }

    // Sobreescribimos CheckAttack para controlar el frenado suave previo al golpe
    protected override void CheckAttack()
    {
        if (PlayerController.Instance == null) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        if (hit != null && !isAttacking && !isStunned)
        {
            StartCoroutine(SensibleAttackRoutine());
        }
    }

    private IEnumerator SensibleAttackRoutine()
    {
        isAttacking = true;
        agent.isStopped = true; // Se detiene antes de atacar

        // Aquí iria la animación de ataque
        // Ejemplo if (anim != null) anim.SetTrigger("attack");

        // Tiempo que tarda el monstruo en estirar o hacer la animacion
        yield return new WaitForSeconds(0.3f);

        // Verificación doble por si el jugador esquivó en ese microsegundo
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);
        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            
            damageable.TakeDamage(attackDamage);
            Debug.Log("<color=red>¡Sensible golpeó al jugador!</color>");
        }
        if (AudioManager.Instance != null && clipGolpe != null)
        {
            // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
            AudioManager.Instance.PlaySFX2D(clipGolpe, 1f);
        }
        // Breve espera para terminar de reproducir el golpe antes del aturdimiento completo
        yield return new WaitForSeconds(0.2f);

        isAttacking = false;
        StartCoroutine(AttackCooldown()); // Pasa a su descanso regular
    }

    private IEnumerator AttackCooldown()
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
        if (type == StimulusType.Corruptible) return;
        base.OnStimulusReceived(position, type);
    }

    private void HandleInvestigation()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            CheckForPlayerProximity();
            investigationTimer -= Time.deltaTime;

            if (investigationTimer <= 0) currentState = State.Wandering;
        }
    }

    private void CheckForPlayerProximity()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (hit != null) currentState = State.Chasing;
    }

    private void Animate()
    {
        anim.SetBool("walking", moving);
        if (moving)
        {
            Vector2 direction = agent.velocity.normalized;
            anim.SetFloat("X", direction.x);
            anim.SetFloat("Y", direction.y);
        }
    }
}