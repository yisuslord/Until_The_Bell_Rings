using UnityEngine;

public class OndaSagrada : BaseItem
{
    [Header("Shockwave Settings")]
    [SerializeField] private float range = 10f;
    [SerializeField] private float pushForce = 15f;
    [SerializeField] private AudioClip clipOnda;

    [SerializeField] private Sprite myIcon;

    // Procesa la logica de reposicionamiento, ejecucion visual, calculo de fuerza fisica y ciclo de vida del clon
    public override void Use()
    {
        // Determinacion de la coordenada exacta del jugador en el frame de activacion
        Vector3 posicionJugador = transform.position;
        PlayerController jugador = Object.FindFirstObjectByType<PlayerController>();

        if (jugador != null)
        {
            posicionJugador = jugador.transform.position;
        }

        // Sincronizacion de posicionamiento global del clon con el cuerpo del jugador
        transform.position = posicionJugador;

        // Reproduccion de la pista de audio antes de alterar los componentes del objeto
        if (AudioManager.Instance != null && clipOnda != null)
        {
            AudioManager.Instance.PlaySFX2D(clipOnda, 1f);
        }

        // Resolucion y ejecucion del componente visual local incorporado en el Prefab
        IVisualEffect effect = GetComponent<IVisualEffect>();
        if (effect == null) effect = GetComponentInChildren<IVisualEffect>();

        if (effect != null)
        {
            effect.PlayEffect(posicionJugador);
        }
        else
        {
            Debug.LogWarning("[OndaSagrada] No se encontro el componente de efecto visual (IVisualEffect) en la estructura de este Prefab.");
        }

        // Calculo de colisiones radiales para la aplicacion del vector de repulsion en los enemigos
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(posicionJugador, range, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out EnemyBase enemy))
            {
                enemy.GetRepelled(posicionJugador, pushForce);
            }
        }

        // NOTA DE DESARROLLO: Remocion de componentes visuales y fisicos superficiales del clon actual.
        // Se preserva la instancia del GameObject en escena durante 2 segundos para garantizar la finalizacion
        // del ciclo de emision de las particulas antes del vaciado definitivo de memoria.
        if (TryGetComponent(out SpriteRenderer sr)) sr.enabled = false;
        if (TryGetComponent(out Collider2D col)) col.enabled = false;

        Destroy(gameObject, 2.0f);
    }
}