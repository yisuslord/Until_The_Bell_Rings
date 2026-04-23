using UnityEngine;
using System.Collections;

public class Manifestado : EnemyBase
{
    [Header("Manifestado Logic")]
    [SerializeField] private AltarZone altarZone;
    [SerializeField] private float darknessThreshold = 3f;
    [SerializeField] private float attackCooldown = 4f; // Un poco m�s de tiempo para que de miedo

    public Animator anim;
    private float darknessTimer;
    private bool isHunting = false;
    private bool isAturdido = false;

    private bool moving;

    private FlashlightController playerFlashlight;
    private SpriteRenderer spriteRenderer; // Para controlar la visibilidad

    protected override void Awake()
    {
        base.Awake();
        playerFlashlight = Object.FindFirstObjectByType<FlashlightController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        moving  = agent.velocity.magnitude > 0.1f;
        Animate();
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // 1. ZONA SEGURA: Si entra al altar, desaparece visualmente y deja de cazar
        if (altarZone != null && altarZone.IsPlayerInside)
        {
            StopHunting();
            spriteRenderer.enabled = false; // Se oculta en el altar
            return;
        }

        // 2. L�GICA DE LINTERNA (Solo si no est� aturdido)
        bool isLightOn = (playerFlashlight != null && playerFlashlight.IsOn);

        if (!isLightOn && !isAturdido)
        {
            darknessTimer += Time.deltaTime;
            if (darknessTimer >= darknessThreshold)
            {
                isHunting = true;
                spriteRenderer.enabled = true; // ?? APARECE cuando empieza a cazar
            }
        }
        else
        {
            StopHunting();
            // Si la luz est� prendida y no est� cazando, se desvanece
            if (!isHunting && !isAturdido) spriteRenderer.enabled = false;
        }

        // 3. ACCI�N DE CAZA
        if (isHunting && PlayerController.Instance != null)
        {
            agent.isStopped = false;
            agent.SetDestination(PlayerController.Instance.transform.position);
            CheckAttack();
        }
        else
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
        }
    }

    protected override void PerformAttack()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackDistance, playerLayer);

        if (hit != null && hit.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(attackDamage);
            Debug.Log("<color=purple>El Manifestado te golpe� y se fundi� en las sombras.</color>");

            // Iniciar el estado de aturdimiento invisible
            StartCoroutine(AturdimientoRoutine());
        }
    }

    private IEnumerator AturdimientoRoutine()
    {
        isAturdido = true;
        isHunting = false;
        darknessTimer = 0;

        spriteRenderer.enabled = false; // ?? SE HACE INVISIBLE tras el golpe

        if (agent.isOnNavMesh) agent.isStopped = true;

        yield return new WaitForSeconds(attackCooldown);

        isAturdido = false;
        // No lo hacemos visible aqu�, esperaremos a que el Update 
        // detecte oscuridad de nuevo para poner spriteRenderer.enabled = true
    }

    private void StopHunting()
    {
        isHunting = false;
        darknessTimer = 0;
        if (agent.isOnNavMesh && !isAturdido) agent.isStopped = true;
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