using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Asechador : EnemyBase
{
    [Header("Asechador Settings")]
    public Animator anim;

    // Conexión al script Altar para poder restarle vida directamente cuando lo golpea.
    [SerializeField] private Altar altarTarget;

    // Conexión a la zona del altar para verificar mediante un booleano si el jugador está dentro de su área segura.
    [SerializeField] private AltarZone altarZone;

    // Frecuencia en segundos con la que el enemigo evalúa si lanzar un ataque al altar.
    [SerializeField] public float attemptInterval = 10f;

    // Porcentaje de éxito de la tirada de probabilidad para iniciar el ataque (ajustable desde LevelManager).
    [Range(0, 100)][SerializeField] public float attackChance = 40f;

    // Cantidad de puntos de vida que se le restan al altar por cada impacto exitoso.
    [SerializeField] private int altarDamage = 15;

    private float attemptTimer;
    private bool moving;
    private bool isAttackingAltar = false;

    [Header("Audio System")]
    // Clips de sonido vinculados al sistema global AudioManager.
    [SerializeField] private AudioClip clipAtaque;
    [SerializeField] private AudioClip clipGolpe;

    // Componente Light2D nativo de URP para la alerta visual del jefe.
    private Light2D miLuz;

    // Candado lógico que previene fallos donde el sonido de ataque se reproduzca repetidamente en un mismo frame.
    private bool yaSonoAtaque = false;

    protected override void Awake()
    {
        // Se ejecuta la inicialización base de EnemyBase para configurar el NavMeshAgent.
        base.Awake();
        attemptTimer = attemptInterval;

        // Búsqueda automatizada para evitar dependencias manuales en el inspector o problemas si el prefab se instancia dinámicamente.
        miLuz = GetComponentInChildren<Light2D>();

        // El enemigo inicia de forma sigilosa, por lo que su luz debe estar apagada en el estado de patrulla inicial.
        ControlarLuz(false);
    }

    private void Update()
    {
        moving = agent.velocity.magnitude > 0.1f;
        Animate();

        // Prioridad absoluta: Si el jefe ya decidió destruir el altar, ignora cualquier otra lógica o patrulla.
        if (isAttackingAltar)
        {
            HandleAltarAttack();
            return;
        }

        // Lógica de estados heredada y adaptada para el comportamiento pasivo del jefe.
        switch (currentState)
        {
            case State.Wandering:
                HandleWandering();
                CheckAltarAttempt();
                break;
            case State.Investigating:
                HandleWandering();
                break;
        }
    }

    private void CheckAltarAttempt()
    {
        // Temporizador interno para espaciar los intentos de ataque y no sobrecargar el procesador en cada frame.
        attemptTimer -= Time.deltaTime;
        if (attemptTimer <= 0)
        {
            attemptTimer = attemptInterval;

            // Regla de diseño: El jefe solo ataca el altar si el jugador lo dejó descuidado (fuera de la zona).
            if (altarZone != null && !altarZone.IsPlayerInside)
            {
                // Sistema basado en probabilidades para dar variedad orgánica a las partidas.
                float roll = Random.Range(0f, 100f);
                Debug.Log($"<color=yellow>Acechador pensando en atacar... Dado: {roll} / Probabilidad: {attackChance}</color>");

                if (roll <= attackChance)
                {
                    Debug.Log("<color=red>¡El Acechador va a atacar el altar!</color>");
                    isAttackingAltar = true;

                    // Activación de la alerta visual para avisar al jugador a la distancia que el altar corre peligro.
                    ControlarLuz(true);

                    // Conexión con el AudioManager global para reproducir el grito del jefe usando el candado de seguridad.
                    if (!yaSonoAtaque && AudioManager.Instance != null && clipAtaque != null)
                    {
                        AudioManager.Instance.PlaySFX2D(clipAtaque, 0.35f);
                        yaSonoAtaque = true;
                    }

                    // Se sobrescribe la ruta del NavMeshAgent para dirigir al enemigo directo a las coordenadas del altar.
                    if (altarTarget != null)
                    {
                        MoveTo(altarTarget.transform.position);
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

        // Medición de distancia constante en espacio 2D hacia el objetivo físico del altar.
        float distanceToAltar = Vector2.Distance(transform.position, altarTarget.transform.position);

        // Verificación de NavMesh: Si el agente terminó de calcular su ruta y está en rango de ataque.
        if (!agent.pathPending && distanceToAltar <= attackDistance)
        {
            // Conexión directa para restar salud al script del Altar.
            altarTarget.TakeDamage(altarDamage);
            Debug.Log("<color=red>¡El Acechador asestó un golpe al altar!</color>");

            // Conexión con el AudioManager para reproducir el impacto físico en el momento exacto del daño.
            if (AudioManager.Instance != null && clipGolpe != null)
            {
                AudioManager.Instance.PlaySFX2D(clipGolpe, 0.8f);
            }

            // Reseteo completo de variables para regresar al comportamiento pasivo de patrulla.
            isAttackingAltar = false;
            yaSonoAtaque = false;
            ControlarLuz(false);

            currentState = State.Wandering;
            attemptTimer = attemptInterval;
        }
    }

    public override void OnStimulusReceived(Vector2 position, StimulusType type)
    {
        // Decisión de diseño: Al ser un jefe enfocado en el altar, ignoramos los ruidos o luces del jugador para que no se distraiga de su objetivo principal.
        return;
    }

    private void ControlarLuz(bool encender)
    {
        // Condición de seguridad que evita errores de referencia nula en consola si el prefab no tiene una luz asignada.
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
            // Normalización del vector de velocidad del NavMesh para alimentar correctamente el Blend Tree de animaciones en 2D (X, Y).
            Vector2 direction = agent.velocity.normalized;
            anim.SetFloat("X", direction.x);
            anim.SetFloat("Y", direction.y);
        }
    }
}