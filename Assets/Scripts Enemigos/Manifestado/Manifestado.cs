using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Manifestado : EnemyBase
{
    [Header("Manifestado Logic")]
    [SerializeField] private AltarZone altarZone; // Enlace al área segura para que el enemigo se disipe instantáneamente si el jugador entra allí.
    [SerializeField] public float darknessThreshold = 3f; // Tiempo en segundos que el jugador debe permanecer a oscuras antes de que este enemigo aparezca.
    [SerializeField] private float attackCooldown = 4f; // Tiempo de espera en las sombras tras realizar una acción de ataque.

    [Header("Audio System")]
    [SerializeField] private AudioClip clipAwake;
    [SerializeField] private AudioClip clipGolpe;

    public Animator anim;
    private float darknessTimer;
    private bool isHunting = false;
    private bool isAturdido = false;
    private bool isAttacking = false;

    private bool moving;
    private bool yaSonoPersecucion = false;

    // Conexiones de lectura directa hacia los componentes lógicos del jugador para analizar su linterna y aspecto visual.
    private FlashlightController playerFlashlight;
    private SpriteRenderer spriteRenderer;

    // Iluminación interna 2D propia asignada a la presencia fantasmal de este monstruo.
    private Light2D miLuz;

    protected override void Awake()
    {
        base.Awake();
        // Localización automática de referencias cruzadas entre objetos de la escena para independizar el prefab.
        playerFlashlight = Object.FindFirstObjectByType<FlashlightController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        miLuz = GetComponentInChildren<Light2D>();

        // El enemigo inicia completamente oculto e intangible en el escenario.
        ControlarLuz(false);
    }

    private void Update()
    {
        // Control riguroso de movimiento: Solo se considera que se desplaza si el agente está activo, posicionado en la malla de navegación y con velocidad física real.
        moving = !isAturdido && !isAttacking && agent.enabled && agent.isOnNavMesh && agent.velocity.magnitude > 0.1f;
        Animate();

        if (!agent.enabled || !agent.isOnNavMesh) return;

        // Mecánica de Zona de Altar: Desvanece al enemigo por completo si el jugador logra refugiarse a tiempo en la luz sagrada.
        if (altarZone != null && altarZone.IsPlayerInside)
        {
            if (isHunting || spriteRenderer.enabled)
            {
                StopHunting();
                spriteRenderer.enabled = false;
                ControlarLuz(false);
                if (agent.isOnNavMesh) agent.ResetPath();
            }
            return;
        }

        if (isAttacking || isAturdido) return;

        // Conexión lógica con el controlador de la linterna: Evaluamos si el jugador tiene la luz encendida.
        bool isLightOn = (playerFlashlight != null && playerFlashlight.IsOn);

        // Mecánica de Oscuridad: Si el jugador apaga la linterna, este enemigo acumula tiempo de carga para manifestarse físicamente.
        if (!isLightOn && !isAturdido)
        {
            darknessTimer += Time.deltaTime;
            if (darknessTimer >= darknessThreshold)
            {
                isHunting = true;
                spriteRenderer.enabled = true;
                ControlarLuz(true); // Al manifestarse visualmente, enciende su luz espectral de acompañamiento.
            }
        }
        else
        {
            // Decisión de diseño: La linterna directa del jugador actúa como repelente natural, forzando al enemigo a volver a ocultarse.
            StopHunting();
            if (!isHunting && !isAturdido) spriteRenderer.enabled = false;
        }

        // Gestión de la secuencia de persecución una vez manifestado.
        if (isHunting)
        {
            if (!yaSonoPersecucion)
            {
                if (AudioManager.Instance != null && clipAwake != null)
                {
                    AudioManager.Instance.PlaySFX2D(clipAwake, 0.25f);
                }
                yaSonoPersecucion = true;
            }

            HandleHunting();
        }
        else
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
        }
    }

    private void HandleHunting()
    {
        if (PlayerController.Instance != null)
        {
            // Comprobación secundaria: Si el jugador usa mecánicas de sigilo para esconderse en armarios u objetos interactivos.
            PlayerHide playerHide = PlayerController.Instance.GetComponent<PlayerHide>();
            if (playerHide != null && playerHide.IsHidden)
            {
                StopHunting();
                spriteRenderer.enabled = false;

                if (agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.isStopped = true;
                }
                return;
            }

            // Comprobación por distancia máxima para desactivar el comportamiento si el jugador se alejó demasiado de la zona.
            float distanciaAlJugador = Vector3.Distance(transform.position, PlayerController.Instance.transform.position);
            if (distanciaAlJugador > 15f)
            {
                StopHunting();
                spriteRenderer.enabled = false;
                return;
            }

            agent.isStopped = false;
            MoveTo(PlayerController.Instance.transform.position);
            CheckAttack();
        }
    }

    protected override void CheckAttack()
    {
        if (PlayerController.Instance == null) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        if (hit != null && !isAttacking && !isAturdido)
        {
            StartCoroutine(ManifestadoAttackRoutine());
        }
    }

    private IEnumerator ManifestadoAttackRoutine()
    {
        isAttacking = true;
        anim.SetBool("Atack", true);
        agent.isStopped = true;

        if (anim != null) anim.SetTrigger("attack");

        yield return new WaitForSeconds(0.3f);

        if (AudioManager.Instance != null && clipGolpe != null)
        {
            AudioManager.Instance.PlaySFX2D(clipGolpe, 1f);
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);
        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            // Aplica daño al jugador y simula un desvanecimiento inmediato en la oscuridad por motivos de ambientación y terror.
            damageable.TakeDamage(attackDamage);
            Debug.Log("<color=purple>El Manifestado te golpeó y se fundió en las sombras.</color>");
        }

        yield return new WaitForSeconds(0.5f);

        isAttacking = false;
        anim.SetBool("Atack", false);
        StartCoroutine(AturdimientoRoutine());
    }

    private IEnumerator AturdimientoRoutine()
    {
        isAturdido = true;
        StopHunting();

        spriteRenderer.enabled = false;

        if (agent.isOnNavMesh) agent.isStopped = true;

        // Cooldown obligatorio ajustable que dicta cuánto tiempo pasará antes de que pueda volver a materializarse en el mapa.
        yield return new WaitForSeconds(attackCooldown);

        isAturdido = false;
    }

    private void StopHunting()
    {
        // Reseteo interno de parámetros lógicos y temporizadores para asegurar un apagado limpio del comportamiento agresivo.
        isHunting = false;
        darknessTimer = 0;
        yaSonoPersecucion = false;
        ControlarLuz(false);

        if (agent.isOnNavMesh && !isAturdido && !isAttacking) agent.isStopped = true;
    }

    private void ControlarLuz(bool encender)
    {
        if (miLuz != null)
        {
            miLuz.enabled = encender;
        }
    }

    private void Animate()
    {
        anim.SetBool("Moving", moving);
        if (moving)
        {
            Vector2 direction = agent.velocity.normalized;
            anim.SetFloat("X", direction.x);
            anim.SetFloat("Y", direction.y);
        }
    }

    // Decisión de diseño: Sobrescribimos el método heredado dejándolo en blanco para inmunizar a este monstruo contra las ondas de repulsión mientras sea intangible.
    public override void GetRepelled(Vector2 shockwaveSource, float force) { }
}