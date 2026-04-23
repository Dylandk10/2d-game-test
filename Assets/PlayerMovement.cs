using System.Collections;
using System.Collections.Generic;
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

    public float MoveInput;
    public Vector2 Velocity => rb.linearVelocity;
    private PlayerAnimation playerAnimatorScript;

    private bool jumpRequested;

    public bool IsGrounded;
    private bool wasGrounded;

    private int jumpCount;

    private bool canAttack = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimatorScript = GetComponent<PlayerAnimation>();
    }

    void Update()
    {
        GetMovement();
        GetAttack();
        CheckGrounded();
    }

    void FixedUpdate()
    {
        ApplyPhysics();
    }

    void GetMovement()
    {
        // Horizontal input
        MoveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            MoveInput = -1f;
        else if (Keyboard.current.dKey.isPressed)
            MoveInput = 1f;

        // Jump input (buffered)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }
    }

    void CheckGrounded()
    {
        IsGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Reset jumps only when landing
        if (!wasGrounded && IsGrounded)
        {
            jumpCount = 0;
        }

        wasGrounded = IsGrounded;
    }

    void ApplyPhysics()
    {
        // Horizontal movement
        rb.linearVelocity = new Vector2(MoveInput * moveSpeed, rb.linearVelocity.y);

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

    void GetAttack()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame && canAttack)
        {
            string[] attacks = { "Attack1", "Attack2", "Attack3" };

            string selected = attacks[Random.Range(0, attacks.Length)];

            playerAnimatorScript.UpdateAttack(selected);

            StartCoroutine(AttackCooldown());
        }
    }

    IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(0.3f);
        canAttack = true;
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
