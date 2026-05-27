using UnityEngine;
using System.Collections;

public class Sensible : EnemyBase
{
    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 4f; // Rango visual/físico para enganchar al jugador en persecución.
    [SerializeField] private AltarZone altarZone; // Conexión con la zona del altar para heredar sus reglas de zona segura.

    [Header("Combat Settings")]
    [SerializeField] private float cooldownTime = 3f; // Tiempo que el enemigo permanece vulnerable/aturdido tras lanzar un golpe.
    public Animator anim;
    private bool moving;
    private bool isStunned = false;
    private bool isAttacking = false;

    [Header("Audio System")]
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipGolpe;

    // Candado lógico para evitar que el grito de persecución se reproduzca infinitamente en bucle durante cada frame del Update.
    private bool yaSonóPersecucion = false;

    private void Update()
    {
        // Control de estados de movimiento para asegurar que las animaciones de caminado no se activen si el enemigo está aturdido o atacando.
        moving = !isStunned && !isAttacking && agent.velocity.magnitude > 0.1f;
        Animate();

        if (isStunned || isAttacking) return;

        // Decisión de diseño: Si el jugador ingresa al Altar, el enemigo pierde el rastro de forma inmediata por motivos de balance de juego.
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
                SetRandomDestination();
            }
        }

        // Ejecución de la máquina de estados específica de este monstruo.
        switch (currentState)
        {
            case State.Wandering: HandleWandering(); break;
            case State.Investigating: HandleInvestigation(); break;
            case State.Chasing: HandleChasing(); break;
        }
    }

    private void HandleChasing()
    {
        // Interconexión con AudioManager para alertar acústicamente al jugador usando el candado de una sola reproducción.
        if (!yaSonóPersecucion)
        {
            if (AudioManager.Instance != null && clipAtaque != null)
            {
                AudioManager.Instance.PlaySFX2D(clipAtaque, 0.2f);
            }
            yaSonóPersecucion = true;
        }

        if (PlayerController.Instance != null)
        {
            // Condición de ruptura 1: El jugador entró a la zona protegida del altar en plena persecución.
            if (altarZone != null && altarZone.IsPlayerInside)
            {
                yaSonóPersecucion = false;
                currentState = State.Wandering;
                return;
            }

            // Condición de ruptura 2: Interconexión con el sistema de sigilo (PlayerHide) para perder el rastro si el jugador se esconde.
            PlayerHide playerHide = PlayerController.Instance.GetComponent<PlayerHide>();
            if (playerHide != null && playerHide.IsHidden)
            {
                yaSonóPersecucion = false;
                currentState = State.Wandering;
                return;
            }

            // Condición de ruptura 3: El jugador logró alejarse lo suficiente corriendo para romper la visión del monstruo.
            float distanciaAlJugador = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
            if (distanciaAlJugador > 15f)
            {
                yaSonóPersecucion = false;
                currentState = State.Wandering;
                return;
            }

            // Si pasa todos los filtros de escape, actualiza la posición del NavMeshAgent persiguiendo las coordenadas del jugador.
            MoveTo(PlayerController.Instance.transform.position);
            CheckAttack();
        }
    }

    protected override void CheckAttack()
    {
        if (PlayerController.Instance == null) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        // Control de flujo: Si detecta colisión con el jugador, inicia la secuencia de ataque por corrutina para controlar tiempos con precisión.
        if (hit != null && !isAttacking && !isStunned)
        {
            StartCoroutine(SensibleAttackRoutine());
        }
    }

    private IEnumerator SensibleAttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("Atack", true);
        Debug.Log("<color=red>¡Sensible inicia su ataque!</color>");

        // Decisión de diseño: Se congela el movimiento del agente para que el ataque sea estático y dé oportunidad de esquivarlo.
        agent.isStopped = true;

        // Tiempo de anticipación sincronizado con el cuadro de la animación donde se asesta el golpe físico.
        yield return new WaitForSeconds(0.5f);

        if (AudioManager.Instance != null && clipGolpe != null)
        {
            AudioManager.Instance.PlaySFX2D(clipGolpe, 2f);
        }

        // Segunda validación de colisión: Comprueba si el jugador sigue ahí o si esquivó exitosamente en el último instante.
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);
        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log("<color=red>¡Sensible golpeó al jugador!</color>");
        }
        else
        {
            Debug.Log("<color=yellow>¡Sensible falló el ataque, el jugador esquivó!</color>");
        }

        // Breve pausa para asimilar el impacto antes de transicionar a la rutina de aturdimiento.
        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        anim.SetBool("Atack", false);
        Debug.Log("<color=blue>Sensible se aturde después de atacar.</color>");
        StartCoroutine(AttackCooldown());
    }

    private IEnumerator AttackCooldown()
    {
        isStunned = true;
        agent.isStopped = true;

        // Estado de vulnerabilidad temporal que premia al jugador por esquivar con éxito el ataque del monstruo.
        Debug.Log($"Sensible descansando por {cooldownTime} segundos...");
        yield return new WaitForSeconds(cooldownTime);

        isStunned = false;
        agent.isStopped = false;
        currentState = State.Wandering;
    }

    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // Restricción lógica: Este enemigo ignora estímulos de tipo corruptible debido a que su diseño está enfocado únicamente en la persecución directa del jugador.
        if (type == StimulusType.Corruptible) return;
        base.OnStimulusReceived(position, type);
    }

    private void HandleInvestigation()
    {
        // Si el enemigo fue distraído o regresó a investigar, liberamos el candado de sonido de caza para dejarlo listo para un futuro encuentro.
        if (yaSonóPersecucion)
        {
            yaSonóPersecucion = false;
        }
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            CheckForPlayerProximity();
            investigationTimer -= Time.deltaTime;

            if (investigationTimer <= 0) currentState = State.Wandering;
        }
    }

    private void CheckForPlayerProximity()
    {
        // Escaneo radial constante en el punto investigado para ver si encuentra al jugador escondido cerca de la perturbación.
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