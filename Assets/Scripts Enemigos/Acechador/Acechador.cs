using UnityEngine;

public class Asechador : EnemyBase
{
    [Header("Asechador Settings")]
    [SerializeField] private Altar altarTarget;
    [SerializeField] private AltarZone altarZone;
    [SerializeField] private float attemptInterval = 10f; // Cada 10s piensa si atacar
    [Range(0, 100)][SerializeField] private float attackChance = 40f; // 40% de probabilidad de éxito al intentar
    [SerializeField] private int altarDamage = 15; // Cuánto le quita al altar de un golpe

    private float attemptTimer;
    private bool isAttackingAltar = false;

    protected override void Awake()
    {
        base.Awake();
        attemptTimer = attemptInterval;
    }

    private void Update()
    {
        // Si ya decidió atacar el altar, ignoramos el patrullaje normal
        if (isAttackingAltar)
        {
            HandleAltarAttack();
            return;
        }

        // Lógica normal (patrullar)
        switch (currentState)
        {
            case State.Wandering:
                HandleWandering();
                CheckAltarAttempt(); // Mientras patrulla, calcula si puede ir al altar
                break;
            case State.Investigating:
                // Como bloqueamos los estímulos abajo, rara vez entrará aquí, 
                // pero lo dejamos por si luego quieres que investigue algo específico.
                HandleWandering();
                break;
        }
    }

    private void CheckAltarAttempt()
    {
        attemptTimer -= Time.deltaTime;
        if (attemptTimer <= 0)
        {
            attemptTimer = attemptInterval; // Reiniciamos el reloj

            // Condición 1: El jugador NO está en la zona del altar
            if (!altarZone.IsPlayerInside)
            {
                // Condición 2: Tiramos los dados (Probabilidad)
                float roll = Random.Range(0f, 100f);
                Debug.Log($"<color=yellow>Acechador pensando en atacar... Dado: {roll} / Probabilidad: {attackChance}</color>");

                if (roll <= attackChance)
                {
                    Debug.Log("<color=red>¡El Acechador va a atacar el altar!</color>");
                    isAttackingAltar = true;
                    MoveTo(altarTarget.transform.position); // Corre hacia el altar
                }
            }
            else
            {
                Debug.Log("<color=grey>Acechador no ataca: El jugador está en la zona del altar.</color>");
            }
        }
    }

    private void HandleAltarAttack()
    {
        // Revisamos si ya llegó a la posición del altar
        float distanceToAltar = Vector2.Distance(transform.position, altarTarget.transform.position);

        if (!agent.pathPending && distanceToAltar <= attackDistance)
        {
            // Golpeamos el altar
            altarTarget.TakeDamage(altarDamage);
            Debug.Log("<color=red>¡El Acechador asestó un golpe al altar!</color>");

            // Volvemos a la normalidad (A patrullar)
            isAttackingAltar = false;
            currentState = State.Wandering;
            attemptTimer = attemptInterval; // Reseteamos el cooldown para que no ataque dos veces seguidas
        }
    }

    // 🔥 LA SOLUCIÓN: Sobreescribimos los estímulos para que el jefe no se distraiga
    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // El Acechador es el jefe. No le importan los ruiditos del jugador ni las velas.
        // Simplemente hacemos un 'return' vacío para que ignore todo.
        return;
    }
}