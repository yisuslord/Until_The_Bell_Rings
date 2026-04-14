using UnityEngine;
using System.Collections;

public class Manifestado : EnemyBase
{
    [Header("Manifestado Logic")]
    [SerializeField] private AltarZone altarZone;
    [SerializeField] private float darknessThreshold = 3f;
    [SerializeField] private float attackCooldown = 4f; // Un poco más de tiempo para que de miedo

    private float darknessTimer;
    private bool isHunting = false;
    private bool isAturdido = false;

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
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // 1. ZONA SEGURA: Si entra al altar, desaparece visualmente y deja de cazar
        if (altarZone != null && altarZone.IsPlayerInside)
        {
            StopHunting();
            spriteRenderer.enabled = false; // Se oculta en el altar
            return;
        }

        // 2. LÓGICA DE LINTERNA (Solo si no está aturdido)
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
            // Si la luz está prendida y no está cazando, se desvanece
            if (!isHunting && !isAturdido) spriteRenderer.enabled = false;
        }

        // 3. ACCIÓN DE CAZA
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
            Debug.Log("<color=purple>El Manifestado te golpeó y se fundió en las sombras.</color>");

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
        // No lo hacemos visible aquí, esperaremos a que el Update 
        // detecte oscuridad de nuevo para poner spriteRenderer.enabled = true
    }

    private void StopHunting()
    {
        isHunting = false;
        darknessTimer = 0;
        if (agent.isOnNavMesh && !isAturdido) agent.isStopped = true;
    }

    public override void GetRepelled(Vector2 shockwaveSource, float force) { }
}