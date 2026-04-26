using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // movement constants
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;

    //ground check
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;


    // components
    private Rigidbody2D rb;
    private PlayerAnimation playerAnimatorScript;
    public Vector2 Velocity => rb.linearVelocity;

    //jumping
    [Header("Jump Settings")]
    public int maxJumps = 2;
    private bool jumpRequested;
    public bool IsGrounded;
    private bool wasGrounded;
    private int jumpCount;
    public float MoveInput;

    //dashing
    public float dashDistance = 6f;
    public float dashSpeed = 10f;
    public float dashCooldown = 6f;
    private bool isDashing = false;
    private bool dashRequested;
    private float lastDashTime;
    private int facingDirection = 1;

    //attacking
    private bool canAttack = true;

    public static PlayerMovement Instance { get; private set; }
    public bool IsBlocking { get; private set; }
    private int health = 100;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional: persist between scenes
        DontDestroyOnLoad(gameObject);


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
        {
            MoveInput = -1f;
            facingDirection = -1;
        }
        else if (Keyboard.current.dKey.isPressed)
        {
            MoveInput = 1f;
            facingDirection = 1;
        }

        // Jump input (buffered)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        // Dash input
        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            dashRequested = true;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            IsBlocking = true;
            playerAnimatorScript.UpdateBlock();
        }else
        {
            IsBlocking = false;
        }
    }

    //need to handle block logic;

    void HandleDash()
    {
        if ((dashRequested && Time.time >= lastDashTime + dashCooldown))
        {
            dashRequested = false;
            lastDashTime = Time.time;

            float direction = MoveInput != 0 ? MoveInput : facingDirection;
            playerAnimatorScript.UpdateDash();

            StartCoroutine(Dash(direction));
        }
    }

    IEnumerator Dash(float direction)
    {
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDistance / dashSpeed);

        rb.gravityScale = originalGravity;
        isDashing = false;
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
        if (isDashing) return;
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
        HandleDash();
    }

    void GetAttack()
    {
        if (isDashing) return;
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
    public void SetBlock(bool value)
    {
        IsBlocking = value;
    }

    public void TakeDamage(int dmg)
    {
        Debug.Log("Player hit (manual check)!");
        Debug.Log(Time.time);
        health -= dmg;
        Debug.Log("Player HP: " + health);
        playerAnimatorScript.UpdateHurt();
    }

    public void Block()
    {
        Debug.Log("Player has blocked!!");
        Debug.Log(Time.time);
    }


}
