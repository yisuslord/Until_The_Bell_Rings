using UnityEngine;
using UnityEngine.Rendering.Universal; // Enlazamos el sistema de luces 2D de Unity (URP)

public class Asechador : EnemyBase
{
    [Header("Asechador Settings")]
    public Animator anim;
    [SerializeField] private Altar altarTarget;
    [SerializeField] private AltarZone altarZone;
    [SerializeField] public float attemptInterval = 10f; // Cada 10s piensa si atacar
    [Range(0, 100)][SerializeField] public float attackChance = 40f; // 40% de probabilidad de éxito al intentar
    [SerializeField] private int altarDamage = 15; // Cuánto le quita al altar de un golpe

    private float attemptTimer;
    private bool moving;
    private bool isAttackingAltar = false;

    [Header("Audio System")]
    [SerializeField] private AudioClip clipAtaque; // Sonido cuando decide correr al Altar
    [SerializeField] private AudioClip clipGolpe;  // Sonido del impacto contra el Altar

    // 🔥 NUEVO COMPONENTE DE LUZ
    private Light2D miLuz;

    // 🔥 CANDADO DE AUDIO: Para que el grito de ataque al altar no se duplique en el frame
    private bool yaSonoAtaque = false;

    protected override void Awake()
    {
        base.Awake();
        attemptTimer = attemptInterval;

        // 🔍 Buscamos la luz de forma automática en el prefab o sus hijos
        miLuz = GetComponentInChildren<Light2D>();

        // Empieza apagada mientras patrulla tranquilamente
        ControlarLuz(false);
    }

    private void Update()
    {
        moving = agent.velocity.magnitude > 0.1f;
        Animate();

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
            if (altarZone != null && !altarZone.IsPlayerInside)
            {
                // Condición 2: Tiramos los dados (Probabilidad)
                float roll = Random.Range(0f, 100f);
                Debug.Log($"<color=yellow>Acechador pensando en atacar... Dado: {roll} / Probabilidad: {attackChance}</color>");

                if (roll <= attackChance)
                {
                    Debug.Log("<color=red>¡El Acechador va a atacar el altar!</color>");
                    isAttackingAltar = true;

                    // 🔥 ALERTA VISUAL: Encendemos su luz en el instante que se vuelve agresivo hacia el Altar
                    ControlarLuz(true);

                    // 🔥 ALERTA DE AUDIO: Suena el grito/aviso de ataque una sola vez
                    if (!yaSonoAtaque && AudioManager.Instance != null && clipAtaque != null)
                    {
                        AudioManager.Instance.PlaySFX2D(clipAtaque, 0.35f); // Volumen balanceado
                        yaSonoAtaque = true;
                    }

                    if (altarTarget != null)
                    {
                        MoveTo(altarTarget.transform.position); // Corre hacia el altar
                    }
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
        if (altarTarget == null) return;

        // Revisamos si ya llegó a la posición del altar
        float distanceToAltar = Vector2.Distance(transform.position, altarTarget.transform.position);

        if (!agent.pathPending && distanceToAltar <= attackDistance)
        {
            // Golpeamos el altar
            altarTarget.TakeDamage(altarDamage);
            Debug.Log("<color=red>¡El Acechador asestó un golpe al altar!</color>");

            // 🔥 CORRECCIÓN DE AUDIO: Vinculado correctamente a clipGolpe y ejecutado al impacto
            if (AudioManager.Instance != null && clipGolpe != null)
            {
                AudioManager.Instance.PlaySFX2D(clipGolpe, 0.8f);
            }

            // Volvemos a la normalidad (A patrullar tranquilamente)
            isAttackingAltar = false;
            yaSonoAtaque = false; // 🔄 RESETEO: Candado listo para la próxima tirada de dados exitosa
            ControlarLuz(false);  // 🔥 APAGADO: Al terminar el ataque y volver a patrullar, apaga su luz

            currentState = State.Wandering;
            attemptTimer = attemptInterval; // Reseteamos el cooldown para que no ataque dos veces seguidas
        }
    }

    // Sobreescribimos los estímulos para que el jefe no se distraiga
    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // El Acechador es el jefe. No le importan los ruiditos del jugador ni las velas.
        return;
    }

    // 🔥 MÉTODO AUXILIAR ANTICRASHEO: Control seguro de la luz
    private void ControlarLuz(bool encender)
    {
        if (miLuz != null)
        {
            miLuz.enabled = encender;
        }
    }

    private void Animate()
    {
        anim.SetBool("moving", moving);
        if (moving)
        {
            Vector2 direction = agent.velocity.normalized;
            anim.SetFloat("X", direction.x);
            anim.SetFloat("Y", direction.y);
        }
    }
}