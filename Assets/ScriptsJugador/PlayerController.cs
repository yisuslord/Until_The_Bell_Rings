using UnityEngine;

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

    [Header("Noise System")]
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;
    private float noiseTimer;
    [SerializeField] private float noiseInterval = 0.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource playerSource;
    [SerializeField] private AudioClip clipCorrer;
    [SerializeField] private AudioClip clipCaminar;
    [SerializeField] private float stepInterval = 0.4f; // Tiempo entre pasos
    private float stepTimer;

    public static PlayerController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        rb = GetComponent<Rigidbody2D>();
        playerHide = GetComponent<PlayerHide>();

        // Si olvidaste asignarlo en el inspector, lo buscamos
        if (playerSource == null) playerSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (playerHide != null && playerHide.IsHidden)
        {
            movementInput = Vector2.zero;
            Animate(); // Para que pase a Idle si se esconde moviéndose
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementInput = new Vector2(horizontal, vertical).normalized;

        isRunning = Input.GetKey(KeyCode.LeftShift);
        isMoving = movementInput != Vector2.zero;

        Animate();
        HandleFootsteps(); // <--- Llamamos a la lógica de audio aquí
    }

    private void HandleFootsteps()
    {
        // Si el jugador se está moviendo y NO está escondido
        if (isMoving && !(playerHide != null && playerHide.IsHidden))
        {
            // 1. Elegir el clip correcto (Caminar o Correr)
            AudioClip clipDeseado = isRunning ? clipCorrer : clipCaminar;

            // 2. Si el clip cambió (ej. de caminar a correr) o no estaba sonando
            if (playerSource.clip != clipDeseado || !playerSource.isPlaying)
            {
                playerSource.clip = clipDeseado;
                playerSource.loop = true; // Hacemos que el sonido sea continuo
                playerSource.Play();
            }

            // 3. Ajustar la velocidad del sonido (Pitch) según la acción
            // Si corre, el sonido se reproduce más rápido
            playerSource.pitch = isRunning ? 1.3f : 1.0f;
        }
        else
        {
            // 4. Si se detiene, cortamos el sonido inmediatamente
            if (playerSource.isPlaying)
            {
                playerSource.Stop();
                playerSource.clip = null; // Limpiamos para que al volver a empezar detecte el cambio
            }
        }
    }

    private void FixedUpdate()
    {
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
}