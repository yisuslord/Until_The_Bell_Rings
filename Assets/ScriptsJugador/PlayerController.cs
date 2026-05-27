using UnityEngine;
using System;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    public Animator anim;
    private PlayerHide playerHide;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool isMoving;
    private bool isRunning;

    [Header("Stamina System")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrain = 20f;   // Cuanto baja por segundo al correr
    [SerializeField] private float staminaRegen = 15f;   // Cuanto sube por second
    private float currentStamina;
    private bool isExhausted = false; // Bloqueo cuando llega a 0 y no deja correr

    [Header("Noise System")]
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;
    private float noiseTimer;
    [SerializeField] private float noiseInterval = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip clipCorrer;
    [SerializeField] private AudioClip clipCaminar;
    [SerializeField] private float walkStepInterval = 0.45f; // Tiempo entre pasos al caminar
    [SerializeField] private float runStepInterval = 0.28f;  // Tiempo entre pasos al correr
    private float stepTimer;

    [Header("Run Path")]
    [SerializeField] private ParticleSystem Rastro;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        rb = GetComponent<Rigidbody2D>();
        playerHide = GetComponent<PlayerHide>();

        // Empezamos con la barra de estamina llena
        currentStamina = maxStamina;
    }

    private void Update()
    {
        // Si el jugador esta escondido, no se mueve, recupera estamina y actualiza las animaciones
        if (playerHide != null && playerHide.IsHidden)
        {
            movementInput = Vector2.zero;
            HandleStamina();
            Animate();
            return;
        }

        // Capturamos el movimiento del teclado o mando
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(horizontal, vertical).normalized;

        isMoving = movementInput != Vector2.zero;

        // Solo puede correr si presiona el boton, se esta moviendo y no se quedo sin estamina
        isRunning = (Input.GetKey(KeyCode.LeftShift) || Input.GetButton("Run")) && isMoving && !isExhausted;

        // Actualizamos todos los sistemas dependientes del movimiento por cada frame
        HandleStamina();
        Animate();
        HandleFootsteps();
        HandleParticles();
    }

    private void HandleParticles()
    {
        if (Rastro == null) return;

        // Prendemos las particulas solo si esta corriendo y moviendose
        var emission = Rastro.emission;
        emission.enabled = isRunning && isMoving;

        if (isRunning && isMoving)
        {
            // Invertimos el movimiento para que las particulas salgan hacia atras
            Vector2 oppositeDirection = -movementInput;

            // Calculamos el angulo en base a la direccion contraria
            float angle = Mathf.Atan2(oppositeDirection.y, oppositeDirection.x) * Mathf.Rad2Deg;

            // Rotamos el emisor de particulas para que apunte en la direccion correcta
            Rastro.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    private void HandleStamina()
    {
        if (isRunning)
        {
            // Si corre, gastamos estamina por segundo
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true; // Se canso, se bloquea la carrera
            }
        }
        else
        {
            // Si camina o esta quieto, recuperamos estamina poco a poco
            currentStamina += staminaRegen * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            // Solo si se recupera al 100% puede volver a correr de nuevo
            if (isExhausted && currentStamina >= maxStamina)
            {
                isExhausted = false;
            }
        }

        // Enviamos los datos actualizados a la interfaz de usuario
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePlayerStamina(currentStamina, maxStamina, isExhausted);
        }
    }

    private void HandleFootsteps()
    {
        if (AudioManager.Instance == null) return;

        if (isMoving)
        {
            // Elegimos el sonido y el volumen dependiendo de si corre o camina
            AudioClip clipDeseado = isRunning ? clipCorrer : clipCaminar;
            float volumen = isRunning ? 0.8f : 0.5f;

            // Mandamos el sonido elegido al manager para que suene en bucle
            AudioManager.Instance.ControlarPasosLoop(clipDeseado, true, volumen);
        }
        else
        {
            // Si se detiene, le avisamos al manager que apague los sonidos de pasos
            AudioManager.Instance.ControlarPasosLoop(null, false, 0f);
        }
    }

    private void FixedUpdate()
    {
        // Aplicamos la velocidad fisica segun el estado actual del jugador
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = movementInput * speed;

        // Si corre, generamos una alerta logica para los enemigos cada cierto tiempo
        if (isMoving && isRunning)
        {
            noiseTimer -= Time.fixedDeltaTime;
            if (noiseTimer <= 0)
            {
                EmitNoise();
                noiseTimer = noiseInterval;
            }
        }
    }

    private void EmitNoise()
    {
        // Buscamos enemigos en un circulo a la redonda y les mandamos el estimulo de ruido
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, noiseRadius, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
            {
                receiver.OnStimulusReceived(transform.position, StimulusType.Noise);
            }
        }
    }

    private void Animate()
    {
        // Controlamos los parametros del Animator para actualizar los sprites del personaje
        anim.SetBool("Moving", isMoving);
        if (isMoving)
        {
            anim.SetFloat("X", movementInput.x);
            anim.SetFloat("Y", movementInput.y);
        }
    }
}