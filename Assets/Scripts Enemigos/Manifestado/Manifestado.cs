using UnityEngine;
using System.Collections;

public class Manifestado : EnemyBase
{
    [Header("Manifestado Logic")]
    [SerializeField] private AltarZone altarZone;
    [SerializeField] public float darknessThreshold = 3f;
    [SerializeField] private float attackCooldown = 4f;

    [Header("Audio System")]
    [SerializeField] private AudioClip clipAwake;
    [SerializeField] private AudioClip clipGolpe;

    public Animator anim;
    private float darknessTimer;
    private bool isHunting = false;
    private bool isAturdido = false;
    private bool isAttacking = false;

    private bool moving;

    private FlashlightController playerFlashlight;
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        playerFlashlight = Object.FindFirstObjectByType<FlashlightController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        moving = !isAturdido && !isAttacking && agent.enabled && agent.isOnNavMesh && agent.velocity.magnitude > 0.1f;
        Animate();

        if (!agent.enabled || !agent.isOnNavMesh) return;

        // 1. ZONA SEGURA: Altar
        if (altarZone != null && altarZone.IsPlayerInside)
        {
            if (isHunting || spriteRenderer.enabled)
            {
                StopHunting();
                spriteRenderer.enabled = false;
                if (agent.isOnNavMesh) agent.ResetPath(); // Limpia la ruta para no campear el Altar
            }
            return;
        }

        if (isAttacking || isAturdido) return;

        // 2. LÓGICA DE LINTERNA (Controla si puede empezar a cazar)
        bool isLightOn = (playerFlashlight != null && playerFlashlight.IsOn);

        if (!isLightOn && !isAturdido)
        {
            darknessTimer += Time.deltaTime;
            if (darknessTimer >= darknessThreshold)
            {
                isHunting = true;
                spriteRenderer.enabled = true;
            }
        }
        else
        {
            StopHunting();
            if (!isHunting && !isAturdido) spriteRenderer.enabled = false;
        }

        // 3. ACCIÓN DE CAZA (Aquí aplicamos tu lógica exacta del Sensible)
        if (isHunting)
        {
            if (AudioManager.Instance != null && clipAwake != null)
            {
                // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
                AudioManager.Instance.PlaySFX2D(clipAwake, 1f);
            }
            HandleHunting();
        }
        else
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
        }
    }

    // 🔥 NUEVO MÉTODO: Estructurado igual que el 'HandleChasing' del Sensible
    private void HandleHunting()
    {
        if (PlayerController.Instance != null)
        {
            // Checar si el jugador tiene el componente y está escondido
            PlayerHide playerHide = PlayerController.Instance.GetComponent<PlayerHide>();
            if (playerHide != null && playerHide.IsHidden)
            {
                StopHunting();            // Apaga el modo caza y el timer
                spriteRenderer.enabled = false; // Se desvanece de inmediato

                if (agent.isOnNavMesh)
                {
                    agent.ResetPath();    // 🔥 LA SOLUCIÓN: Borra la ruta hacia la caja
                    agent.isStopped = true; // Se queda congelado en su posición actual
                }
                return;
            }

            // Si no está escondido, lo persigue normalmente
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
        agent.isStopped = true;

        // Aquí va la animación de ataque
        if (anim != null) anim.SetTrigger("attack");

        yield return new WaitForSeconds(0.3f);

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);
        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            if (AudioManager.Instance != null && clipGolpe != null)
            {
                // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
                AudioManager.Instance.PlaySFX2D(clipGolpe, 1f);
            }
            Debug.Log("<color=purple>El Manifestado te golpeó y se fundió en las sombras.</color>");
        }

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        StartCoroutine(AturdimientoRoutine());
    }

    private IEnumerator AturdimientoRoutine()
    {
        isAturdido = true;
        isHunting = false;
        darknessTimer = 0;

        spriteRenderer.enabled = false;

        if (agent.isOnNavMesh) agent.isStopped = true;

        yield return new WaitForSeconds(attackCooldown);

        isAturdido = false;
    }

    private void StopHunting()
    {
        isHunting = false;
        darknessTimer = 0;
        if (agent.isOnNavMesh && !isAturdido && !isAttacking) agent.isStopped = true;
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

    public override void GetRepelled(Vector2 shockwaveSource, float force) { }
}