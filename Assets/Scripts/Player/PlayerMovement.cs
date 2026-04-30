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
        Attack,
        Hurt
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
    private bool attackRequested = false;

    public float MoveInput;
    public Vector2 Velocity => Player.Instance.GetRigidbody2D().linearVelocity;

    void Awake()
    {
        currentState = PlayerState.Idle;
    }

    void Update()
    {
        GetInput();
        CheckGrounded();
        StateMachine();
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

        if (Player.Instance.GetLives() <= 0)
        {
            return;
        }

        // don't block attack
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            attackRequested = true;
        }
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
        if (attackRequested && canAttack)
        {
            attackRequested = false;
            StartAttack();
        }
    }

    void HandleMove()
    {
        if (attackRequested && canAttack)
        {
            attackRequested = false;
            StartAttack();
        }

        if (dashRequested)
        {
            TryDash();
            return;
        }

        if (jumpRequested)
        {
            HandleJump();
            return;
        }

        if (Mathf.Abs(MoveInput) < 0.1f)
            currentState = PlayerState.Idle;
    }

    void HandleJump() 
    {
        // Jump logic
        if (jumpRequested && jumpCount < maxJumps)
        {
            // Reset vertical velocity for consistent jump height
            Player.Instance.GetRigidbody2D().linearVelocity = new Vector2(Player.Instance.GetRigidbody2D().linearVelocity.x, 0f);
            Player.Instance.GetRigidbody2D().linearVelocity = new Vector2(Player.Instance.GetRigidbody2D().linearVelocity.x, jumpForce);
            if(jumpCount < 1)
                Player.Instance.GetPlayerAnimationScript().UpdateJump();
            else
                Player.Instance.GetPlayerAnimationScript().UpdateDash();
            jumpCount++;
        }
        jumpRequested = false;
    }
    void HandleDashState() 
    {
        attackRequested = false;
        jumpRequested = false;
    }
    void HandleAttackState()
    {
        jumpRequested = false;
        dashRequested = false;
    }

    // ======================================================
    // MOVEMENT
    // ======================================================

    void ApplyMovement()
    {
        if (currentState == PlayerState.Dash)
            return;

        Player.Instance.GetRigidbody2D().linearVelocity = new Vector2(
            MoveInput * moveSpeed,
            Player.Instance.GetRigidbody2D().linearVelocity.y
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
            if (currentState != PlayerState.Attack && currentState != PlayerState.Hurt && currentState != PlayerState.Dash)
            {
                currentState = PlayerState.Idle;
            }
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

        Player.Instance.GetPlayerAnimationScript().UpdateDash();
        StartCoroutine(DashRoutine(dir));
    }

    IEnumerator DashRoutine(float direction)
    {
        float originalGravity = Player.Instance.GetRigidbody2D().gravityScale;
        Player.Instance.GetRigidbody2D().gravityScale = 0f;

        Player.Instance.GetRigidbody2D().linearVelocity =
            new Vector2(direction * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDistance / dashSpeed);

        Player.Instance.GetRigidbody2D().gravityScale = originalGravity;

        currentState = PlayerState.Move;
    }

    // ======================================================
    // ATTACK
    // ======================================================

    public void StartAttack()
    {
        currentState = PlayerState.Attack;
        canAttack = false;

        Player.Instance.ClearHitEnemies();

        string[] attacks = { "Attack1", "Attack2", "Attack3" };
        string selected = attacks[Random.Range(0, attacks.Length)];

        Player.Instance.GetPlayerAnimationScript().UpdateAttack(selected);
    }

    public void EndAttack()
    {
        canAttack = true;
        currentState = PlayerState.Move;
    }

    public bool GetCanDealDamage()
    {
        return canDealDamage;
    }

    public void SetCanDealDamage(bool canDeal)
    {
        canDealDamage = canDeal;
    }

    public bool GetAttackRequest()
    {
        return attackRequested;
    }

    public void SetAttackRequest(bool attack)
    {
        attackRequested = attack;
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
    }
}