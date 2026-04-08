using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    private PlayerHide playerHide;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    private bool isRunning;

    [Header("Noise System")]
    [SerializeField] private float noiseRadius = 5f;
    [SerializeField] private LayerMask enemyLayer;
    private float noiseTimer;
    [SerializeField] private float noiseInterval = 0.5f;
    public static PlayerController Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        rb = GetComponent<Rigidbody2D>();
        playerHide = GetComponent<PlayerHide>();
    }

    private void Update()
    {
        if (playerHide != null && playerHide.IsHidden)
        {
            movementInput = Vector2.zero;
            return;
        }
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        movementInput.x = horizontal;
        movementInput.y = vertical;
        isRunning = Input.GetKey(KeyCode.LeftShift);
        movementInput = movementInput.normalized;
    }

    private void FixedUpdate()
    {
        float speed = isRunning ? runSpeed : walkSpeed;

        rb.linearVelocity = movementInput * speed;

        if (movementInput != Vector2.zero && isRunning)
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
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, noiseRadius, enemyLayer);
        foreach (var hit in hitEnemies)
        {
            if (hit.TryGetComponent(out IStimulusReceiver receiver))
            {
                receiver.OnStimulusReceived(transform.position, StimulusType.Noise);
            }
        }
    }
}