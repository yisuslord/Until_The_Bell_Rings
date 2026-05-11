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
    [SerializeField] private AudioSource playerSource;
    [SerializeField] private AudioClip clipCorrer;
    [SerializeField] private AudioClip clipCaminar;
    private float stepTimer;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        rb = GetComponent<Rigidbody2D>();
        playerHide = GetComponent<PlayerHide>();

        currentStamina = maxStamina; // Empezamos llenos

        if (playerSource == null) playerSource = GetComponent<AudioSource>();
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
        isRunning = Input.GetKey(KeyCode.LeftShift) && isMoving && !isExhausted;

        HandleStamina();
        Animate();
        HandleFootsteps();
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
        if (isMoving && !(playerHide != null && playerHide.IsHidden))
        {
            AudioClip clipDeseado = isRunning ? clipCorrer : clipCaminar;

            if (playerSource.clip != clipDeseado || !playerSource.isPlaying)
            {
                playerSource.clip = clipDeseado;
                playerSource.loop = true;
                playerSource.Play();
            }

            playerSource.pitch = isRunning ? 1.3f : 1.0f;
        }
        else
        {
            if (playerSource.isPlaying)
            {
                playerSource.Stop();
                playerSource.clip = null;
            }
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