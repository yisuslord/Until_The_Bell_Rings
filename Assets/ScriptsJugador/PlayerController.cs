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

    private void Awake()
    {
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

        if (movementInput != Vector2.zero)
        {
            // 🔊 Sistema de ruido YISUS
        }
    }
}