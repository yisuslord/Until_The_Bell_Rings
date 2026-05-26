using UnityEngine;
using System.Collections;
// Nos aseguramos de incluir el namespace de luces si usas el pipeline 2D (URP)
using UnityEngine.Rendering.Universal;

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
    private bool yaSonoPersecucion = false;

    private FlashlightController playerFlashlight;
    private SpriteRenderer spriteRenderer;

    // 🔥 NUEVA COMPONENTE DE LUZ
    private Light2D miLuz;

    protected override void Awake()
    {
        base.Awake();
        playerFlashlight = Object.FindFirstObjectByType<FlashlightController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 🔍 Buscamos la luz de forma automática en el mismo objeto o en sus hijos
        miLuz = GetComponentInChildren<Light2D>();

        // La apagamos por defecto al iniciar el juego por si acaso comenzó encendida en el inspector
        ControlarLuz(false);
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
                ControlarLuz(false); // 🔥 ALTARES: Se apaga la luz al desvanecerse
                if (agent.isOnNavMesh) agent.ResetPath();
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
                ControlarLuz(true); // 🔥 APARICIÓN: Se enciende la luz al manifestarse desde la oscuridad
            }
        }
        else
        {
            // 🔥 ESPANTADO: Si el jugador lo alumbra con la linterna, el método StopHunting() se encargará de apagar su luz
            StopHunting();
            if (!isHunting && !isAturdido) spriteRenderer.enabled = false;
        }

        // 3. ACCIÓN DE CAZA 
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
        StopHunting(); // 🔄 Esto reinicia parámetros y apaga la luz automáticamente por seguridad

        spriteRenderer.enabled = false;

        if (agent.isOnNavMesh) agent.isStopped = true;

        yield return new WaitForSeconds(attackCooldown);

        isAturdido = false;
    }

    private void StopHunting()
    {
        isHunting = false;
        darknessTimer = 0;
        yaSonoPersecucion = false;
        ControlarLuz(false); // 🔥 ESPANTADO / DORMIDO: Apagamos la luz de inmediato en cualquier reseteo del monstruo

        if (agent.isOnNavMesh && !isAturdido && !isAttacking) agent.isStopped = true;
    }

    // 🔥 MÉTODO AUXILIAR ANTICRASHEO: Controla el encendido seguro de la luz
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

    public override void GetRepelled(Vector2 shockwaveSource, float force) { }
}