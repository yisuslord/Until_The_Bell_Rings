using UnityEngine;

public class ShockwaveItem : BaseItem
{
    [Header("Shockwave Settings")]
    [SerializeField] private float range = 10f;
    [SerializeField] private float pushForce = 15f;
    //[SerializeField] private LayerMask enemyLayer;

    public override void Use()
    {
        // 1. Buscamos el efecto visual en los hijos del objeto que nos tiene (el Jugador)
        // El Inventario hace: item.transform.SetParent(player.transform)
        IVisualEffect effect = GetComponentInParent<IVisualEffect>();

        if (effect != null)
        {
            effect.PlayEffect(transform.parent.position); // Posición del jugador
        }
        else
        {
            // Plan B: Buscar en toda la escena si no somos hijos del jugador
            effect = Object.FindFirstObjectByType<Onda_Visual>();
            if (effect != null) effect.PlayEffect(transform.position);
        }

        // 2. Lógica de Empuje
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, range, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out EnemyBase enemy))
            {
                enemy.GetRepelled(transform.position, pushForce);
            }
        }
    }
}