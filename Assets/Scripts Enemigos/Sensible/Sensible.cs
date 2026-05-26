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


    private bool yaSonóPersecucion = false; // 🔥 Candado para que el audio no se repita en bucle

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
        // 🔥 EL CANDADO: Suena exactamente una vez al iniciar esta persecución
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
            // CASO 1: El jugador entra al altar
            if (altarZone != null && altarZone.IsPlayerInside)
            {
                yaSonóPersecucion = false; // 🔄 Reseteamos
                currentState = State.Wandering; // Asegúrate de cambiar el estado aquí si se salva
                return;
            }

            // CASO 2: El jugador se esconde
            PlayerHide playerHide = PlayerController.Instance.GetComponent<PlayerHide>();
            if (playerHide != null && playerHide.IsHidden)
            {
                yaSonóPersecucion = false; // 🔄 Reseteamos
                currentState = State.Wandering;
                return;
            }

            // CASO 3: Verificación de distancia (Por si el jugador lo pierde corriendo normal)
            // Ajusta el "15f" por la distancia máxima de visión/pérdida de tu juego
            float distanciaAlJugador = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
            if (distanciaAlJugador > 15f)
            {
                yaSonóPersecucion = false; // 🔄 Reseteamos porque lo perdió de vista
                currentState = State.Wandering;
                return;
            }

            // Si no se cumple ninguna de las anteriores, lo sigue persiguiendo
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
        anim.SetBool("Atack", true);
        Debug.Log("<color=red>¡Sensible inicia su ataque!</color>");
        agent.isStopped = true; // Se detiene antes de atacar

        // Tiempo que tarda el monstruo en estirar los brazos o hacer la animación
        yield return new WaitForSeconds(0.5f);

        // 🔥 REPRODUCCIÓN BLINDADA: El sonido se ejecuta al completarse el golpe (Volumen subido a 1f)
        if (AudioManager.Instance != null && clipGolpe != null)
        {
            AudioManager.Instance.PlaySFX2D(clipGolpe, 2f);
        }

        // Verificación doble por si el jugador esquivó en ese microsegundo
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

        // Breve espera para terminar de reproducir el golpe antes del aturdimiento completo
        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        anim.SetBool("Atack", false);
        Debug.Log("<color=blue>Sensible se aturde después de atacar.</color>");
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
        // Si el monstruo está patrullando, el candado de audio DEBE estar listo para la próxima persecución
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