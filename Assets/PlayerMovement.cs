using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Move,
        Jump,
        Dash,
        Attack
    }

    public PlayerState currentState;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 12f;
    private bool jumpRequested = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public bool IsGrounded;
    private bool wasGrounded;

    [Header("Jump Settings")]
    public int maxJumps = 2;
    private int jumpCount;

    [Header("Dash")]
    public float dashDistance = 6f;
    public float dashSpeed = 10f;
    public float dashCooldown = 15f;

    private float lastDashTime;
    private bool dashRequested;
    private int facingDirection = 1;

    [Header("Attack")]
    private bool canAttack = true;
    private bool canDealDamage = true;

    [SerializeField] public Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(1.5f, 1f);
    [SerializeField] private LayerMask enemyLayer;

    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();

    public float MoveInput;
    public Vector2 Velocity => Player.Instance.rb.linearVelocity;

    void Awake()
    {
        currentState = PlayerState.Idle;
    }

    void Update()
    {
        GetInput();
        CheckGrounded();
        StateMachine();

        if (currentState == PlayerState.Attack)
            DealDamage();
    }

    void FixedUpdate()
    {
        ApplyMovement();
    }

    // ======================================================
    // INPUT
    // ======================================================

    void GetInput()
    {
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

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            jumpRequested = true;
        }

        if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
        {
            dashRequested = true;
        }
    }

    // ======================================================
    // STATE MACHINE
    // ======================================================

    void StateMachine()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;

            case PlayerState.Move:
                HandleMove();
                break;

            case PlayerState.Jump:
                HandleJump();
                break;

            case PlayerState.Dash:
                HandleDashState();
                break;

            case PlayerState.Attack:
                HandleAttackState();
                break;
        }
    }

    void HandleIdle()
    {
        if (Mathf.Abs(MoveInput) > 0.1f)
            currentState = PlayerState.Move;

        if (dashRequested)
            TryDash();
        if (jumpRequested)
            HandleJump();

        if (Mouse.current.leftButton.wasPressedThisFrame && canAttack)
            StartAttack();
    }

    void HandleMove()
    {
        if (Mathf.Abs(MoveInput) < 0.1f)
            currentState = PlayerState.Idle;

        if (dashRequested)
            TryDash();
        if (jumpRequested)
            HandleJump();

        if (Mouse.current.leftButton.wasPressedThisFrame && canAttack)
            StartAttack();
    }

    void HandleJump() 
    {
        // Jump logic
        if (jumpRequested && jumpCount < maxJumps)
        {
            // Reset vertical velocity for consistent jump height
            Player.Instance.rb.linearVelocity = new Vector2(Player.Instance.rb.linearVelocity.x, 0f);
            Player.Instance.rb.linearVelocity = new Vector2(Player.Instance.rb.linearVelocity.x, jumpForce);
            if(jumpCount < 1)
                Player.Instance.playerAnimatorScript.UpdateJump();
            else
                Player.Instance.playerAnimatorScript.UpdateDash();
            jumpCount++;
        }
        jumpRequested = false;
    }
    void HandleDashState() { }
    void HandleAttackState() { }

    // ======================================================
    // MOVEMENT
    // ======================================================

    void ApplyMovement()
    {
        if (currentState == PlayerState.Dash ||
            currentState == PlayerState.Attack)
            return;

        Player.Instance.rb.linearVelocity = new Vector2(
            MoveInput * moveSpeed,
            Player.Instance.rb.linearVelocity.y
        );
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
            currentState = PlayerState.Idle;
        }

        wasGrounded = IsGrounded;
    }

    // ======================================================
    // DASH
    // ======================================================

    void TryDash()
    {
        if (Time.time < lastDashTime + dashCooldown)
        {
            // reset request otherwise constantly dash.
            dashRequested = false;
            return;
        }

        dashRequested = false;
        lastDashTime = Time.time;

        currentState = PlayerState.Dash;

        float dir = MoveInput != 0 ? MoveInput : facingDirection;

        Player.Instance.playerAnimatorScript.UpdateDash();
        StartCoroutine(DashRoutine(dir));
    }

    IEnumerator DashRoutine(float direction)
    {
        float originalGravity = Player.Instance.rb.gravityScale;
        Player.Instance.rb.gravityScale = 0f;

        Player.Instance.rb.linearVelocity =
            new Vector2(direction * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDistance / dashSpeed);

        Player.Instance.rb.gravityScale = originalGravity;

        currentState = PlayerState.Move;
    }

    // ======================================================
    // ATTACK
    // ======================================================

    void StartAttack()
    {
        currentState = PlayerState.Attack;
        canAttack = false;

        hitEnemies.Clear();

        string[] attacks = { "Attack1", "Attack2", "Attack3" };
        string selected = attacks[Random.Range(0, attacks.Length)];

        Player.Instance.playerAnimatorScript.UpdateAttack(selected);

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canDealDamage = true;

        yield return new WaitForSeconds(0.2f);
        canDealDamage = false;

        yield return new WaitForSeconds(0.1f);

        canAttack = true;
        currentState = PlayerState.Move;
    }

    void DealDamage()
    {
        if (!canDealDamage) return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            attackPoint.position,
            attackSize,
            0f,
            enemyLayer
        );

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponentInParent<Enemy>();

            if (enemy != null && !hitEnemies.Contains(enemy))
            {
                hitEnemies.Add(enemy);
                enemy.TakeDamage(Player.Instance.GetDamage());
            }
        }
    }

    // ======================================================
    // DEBUG
    // ======================================================

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackSize);
        }
    }
}