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
    [SerializeField] private float staminaDrain = 20f;   // Cuánto baja por segundo al correr
    [SerializeField] private float staminaRegen = 15f;   // Cuánto sube por segundo
    private float currentStamina;
    private bool isExhausted = false; // Bloqueo cuando llega a 0

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

        currentStamina = maxStamina; // Empezamos llenos

        //if (playerSource == null) playerSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (playerHide != null && playerHide.IsHidden)
        {
            movementInput = Vector2.zero;
            HandleStamina(); // Recuperar estamina mientras está escondido
            Animate();
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(horizontal, vertical).normalized;
        
        isMoving = movementInput != Vector2.zero;

        // Lógica de Carrera con Estamina
        // Solo puede correr si presiona Shift, se mueve, Y NO está agotado
        isRunning = (Input.GetKey(KeyCode.LeftShift)||Input.GetButton("Run")) && isMoving && !isExhausted;

        HandleStamina();
        Animate();
        HandleFootsteps();
        HandleParticles();
    }

    private void HandleParticles()
    {
        if (Rastro == null) return;

        // Si está corriendo y se está moviendo, activamos la emisión
        var emission = Rastro.emission;

        // Al usar 'enabled', si se apaga, las partículas ya creadas siguen vivas
        emission.enabled = isRunning && isMoving;

        if (isRunning && isMoving)
        {
            // HACER QUE FLUYAN EN DIRECCIÓN OPUESTA
            // Tomamos el vector de movimiento invertido (-movementInput)
            Vector2 oppositeDirection = -movementInput;

            // Convertimos la dirección en un ángulo en grados
            float angle = Mathf.Atan2(oppositeDirection.y, oppositeDirection.x) * Mathf.Rad2Deg;

            // Rotamos el sistema de partículas hacia ese ángulo
            Rastro.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
    private void HandleStamina()
    {
        if (isRunning)
        {
            currentStamina -= staminaDrain * Time.deltaTime;
            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
            }
        }
        else
        {
            currentStamina += staminaRegen * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

            if (isExhausted && currentStamina >= maxStamina)
            {
                isExhausted = false;
            }
        }

        // 🔥 LLAMADA AL UI MANAGER
        // Asumiendo que tu UIManager tiene una instancia estática: UIManager.Instance
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
            // 1. Elegimos qué clip mandarle al canal de SFX
            AudioClip clipDeseado = isRunning ? clipCorrer : clipCaminar;
            float volumen = isRunning ? 0.8f : 0.5f; // Correr suena un poco más fuerte

            // 2. Le decimos al manager: "reproduce este clip en bucle"
            AudioManager.Instance.ControlarPasosLoop(clipDeseado, true, volumen);
        }
        else
        {
            // 3. Si no se mueve, le decimos al manager que apague ese loop de SFX
            AudioManager.Instance.ControlarPasosLoop(null, false, 0f);
        }
    }



    private void FixedUpdate()
    {
        // La velocidad depende de si realmente está corriendo (considerando estamina)
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.linearVelocity = movementInput * speed;

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
        // El sonido de "correr" ya lo maneja HandleFootsteps para el ritmo de pasos.
        // Aquí emitimos la alerta visual/lógica para los enemigos.
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
        anim.SetBool("Moving", isMoving);
        if (isMoving)
        {
            anim.SetFloat("X", movementInput.x);
            anim.SetFloat("Y", movementInput.y);
        }
    }

    // ... (Métodos EmitNoise y Animate se mantienen igual)
}