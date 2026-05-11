using UnityEngine;

public class OndaSagrada : BaseItem, ICorruptible
{
    [Header("Shockwave Settings")]
    [SerializeField] private float range = 10f;
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private AudioClip clipOnda;

    /*private bool isCorrupted = false;

    public bool IsCorrupted => isCorrupted;

    public void Corrupt() {

    }

    public void Restore()
    {

    }*/
    public override void Use()
    {
        // 1. Obtener el AudioSource del Jugador dinámicamente
        // Como el item es hijo del jugador, buscamos en el padre.
        AudioSource pSource = GetComponentInParent<AudioSource>();

        if (pSource != null && clipOnda != null)
        {
            pSource.PlayOneShot(clipOnda);
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