using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Jump Settings")]
    public int maxJumps = 2;

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpRequested;

    private bool isGrounded;
    private bool wasGrounded;

    private int jumpCount;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        GetMovement();
        CheckGrounded();
    }

    void FixedUpdate()
    {
        ApplyPhysics();
    }

    void GetMovement()
    {
        // Horizontal input
        moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveInput = -1f;
        else if (Keyboard.current.dKey.isPressed)
            moveInput = 1f;

        // Jump input (buffered)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Reset jumps only when landing
        if (!wasGrounded && isGrounded)
        {
            jumpCount = 0;
        }

        wasGrounded = isGrounded;
    }

    void ApplyPhysics()
    {
        // Horizontal movement
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

        // Jump logic
        if (jumpRequested && jumpCount < maxJumps)
        {
            // Reset vertical velocity for consistent jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            jumpCount++;
        }

        jumpRequested = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
