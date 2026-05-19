using UnityEngine;

public class OndaSagrada : BaseItem
{
    [Header("Shockwave Settings")]
    [SerializeField] private float range = 10f;
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private AudioClip clipOnda;

    [SerializeField] private Sprite myIcon;

    // Devolvemos el sprite cumpliendo con la interfaz
    //public Sprite InventoryIcon => myIcon;

    

    /*private bool isCorrupted = false;

    public bool IsCorrupted => isCorrupted;

    public void Corrupt() {

    }

    public void Restore()
    {

    }*/
    public override void Use()
    {
        /// 🔥 LLAMADA AL AUDIO MANAGER ANTES DE DESTRUIR EL OBJETO
        if (AudioManager.Instance != null && clipOnda != null)
        {
            // Usamos 2D porque es un sonido de inventario/interfaz para el jugador
            AudioManager.Instance.PlaySFX2D(clipOnda, 1f);
        }

        // 2. Efecto Visual
        IVisualEffect effect = GetComponentInParent<IVisualEffect>();
        if (effect != null)
        {
            effect.PlayEffect(transform.parent.position);
        }
        else
        {
            effect = Object.FindFirstObjectByType<Onda_Visual>();
            if (effect != null) effect.PlayEffect(transform.position);
        }

        // 3. Lógica de Empuje
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