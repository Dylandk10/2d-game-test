using System.Collections;
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

    [Header("Stats")]
    public PlayerStats playerStats;

    public PlayerState currentState;

    [Header("Movement")]
    public float moveSpeed => playerStats.moveSpeed;

    [Header("Jump")]
    public float jumpForce => playerStats.jumpForce;
    private bool jumpRequested = false;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    public bool IsGrounded;
    private bool wasGrounded;

    [Header("Jump Settings")]
    public int maxJumps => playerStats.maxJumps;
    private int jumpCount;

    [Header("Dash")]
    public float dashDistance => playerStats.dashDistance; 
    public float dashSpeed => playerStats.dashSpeed; 
    public float dashCooldown => playerStats.dashCooldown;
    private float originalGravity = 2.5f;
    private float lastDashTime;
    private bool dashRequested;
    private int facingDirection = 1;
    private Coroutine dashRoutine;

    [Header("Attack")]
    private float attackBufferTime = 0.2f;
    private float lastAttackInputTime;
    private int comboIndex = 0;
    private float comboResetTime = 0.5f;
    private bool canAttack = true;
    private float lastAttackEndTime;

    public float MoveInput;
    private Rigidbody2D rb;
    private PlayerAnimation playerAnimationScript;


    [SerializeField] private float knockbackForce = 7f;
    [SerializeField] private float knockbackUpForce = 4f;

    private bool isKnockedBack = false;

    void Awake()
    {
        currentState = PlayerState.Idle;
        rb = GetComponent<Rigidbody2D>();
        playerAnimationScript = GetComponent<PlayerAnimation>();
    }

    void Start()
    {
        lastDashTime = -dashCooldown;
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

        if (isKnockedBack || currentState == PlayerState.Dash)
        {
            // Still allow facing updates, but block actions
            jumpRequested = false;
            dashRequested = false;
            return;
        }

        
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            lastAttackInputTime = Time.time;
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
        bool bufferedAttack = Time.time <= lastAttackInputTime + attackBufferTime;
        if (Mathf.Abs(MoveInput) > 0.2f)
            currentState = PlayerState.Move;

        if (dashRequested)
            TryDash();
        if (jumpRequested)
            HandleJump();
        if (bufferedAttack && canAttack)
        {
            lastAttackInputTime = -999f; // consume it
            StartAttack();
            return;
        }
    }

    void HandleMove()
    {
        bool bufferedAttack = Time.time <= lastAttackInputTime + attackBufferTime;
        if (bufferedAttack && canAttack)
        {
            lastAttackInputTime = -999f; // consume it
            StartAttack();
            return;
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

        if (Mathf.Abs(MoveInput) < 0.2f)
            currentState = PlayerState.Idle;
    }

    void HandleJump() 
    {
        // Jump logic
        if (jumpRequested && jumpCount < maxJumps)
        {
            // Reset vertical velocity for consistent jump height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (jumpCount < 1)
                playerAnimationScript.UpdateJump();
            else
                playerAnimationScript.UpdateJump();
            // playerAnimationScript.UpdateDash();
            //put second jump anim here

            jumpCount++;
        }
        jumpRequested = false;
    }
    void HandleDashState() 
    {
        jumpRequested = false;
    }
    void HandleAttackState()
    {
        if (Time.time <= lastAttackInputTime + attackBufferTime && canAttack)
        {
            lastAttackInputTime = -999f;
            StartAttack();
        }
    }

    // ======================================================
    // MOVEMENT
    // ======================================================

    void ApplyMovement()
    {
        if (currentState == PlayerState.Dash || isKnockedBack)
            return;

        rb.linearVelocity = new Vector2(
            MoveInput * moveSpeed,
            rb.linearVelocity.y
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

        


        float dir = MoveInput != 0 ? MoveInput : facingDirection;
        rb.gravityScale = 0f;
        playerAnimationScript.UpdateDash();
        //StartCoroutine(DashRoutine(dir));
    }

    public void StartMoveDash()
    {
        currentState = PlayerState.Dash;
        float dir = MoveInput != 0 ? MoveInput : facingDirection;
        

        float maxDistance = dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.right * dir, dashDistance, groundLayer);

        if (hit)
        {
            maxDistance = hit.distance;
        }

        float dashTime = maxDistance / dashSpeed;

        rb.linearVelocity = new Vector2(dir * dashSpeed, 0f);

        // 🔥 SAFETY TIMER
        if (dashRoutine != null)
            StopCoroutine(dashRoutine);

        dashRoutine = StartCoroutine(ForceEndDash(dashTime));
    }

    IEnumerator ForceEndDash(float time)
    {
        yield return new WaitForSeconds(time);

        // If animation didn't end it, force stop
        if (currentState == PlayerState.Dash)
        {
            EndDashMovement();
        }
    }

    public void EndDashMovement()
    {
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = originalGravity;

        currentState = PlayerState.Move;
    }




    // ======================================================
    // ATTACK
    // ======================================================
    //
    //General flow

    public void StartAttack()
    {
        string[] attacks = { "Attack1", "Attack2", "Attack3" };

        if (Time.time > lastAttackEndTime + comboResetTime)
        {
            comboIndex = 0;
        }

        string selected = attacks[comboIndex];
        comboIndex = (comboIndex + 1) % attacks.Length;

        //push attack slightly forward
        rb.linearVelocity = new Vector2(facingDirection * 2f, rb.linearVelocity.y);

        currentState = PlayerState.Attack;

        Player.Instance.ClearHitEnemies();

        playerAnimationScript.UpdateAttack(selected);
    }

    public void EndAttack()
    {
        canAttack = true;
        currentState = PlayerState.Move;
        lastAttackEndTime = Time.time;
    }


    // for ui handinign
    public float GetDashCooldownNormalized()
    {
        float t = (Time.time - lastDashTime) / dashCooldown;
        return Mathf.Clamp01(t);
    }

    public bool IsDashReady()
    {
        return Time.time >= lastDashTime + dashCooldown;
    }

    public void ApplyKnockback(Vector2 sourcePosition)
    {
        if (isKnockedBack) return;

        StartCoroutine(KnockbackRoutine(sourcePosition));
    }

    IEnumerator KnockbackRoutine(Vector2 sourcePosition)
    {
        isKnockedBack = true;
        currentState = PlayerState.Hurt;

        float dir = transform.position.x < sourcePosition.x ? -1f : 1f;

        // 🔥 INSTANT snap velocity (this is the magic)
        rb.linearVelocity = new Vector2(dir * knockbackForce, knockbackUpForce);

        float timer = 0f;

        while (timer < 0.2f)
        {
            timer += Time.deltaTime;

            // 🔥 maintain horizontal force (prevents drag slowdown)
            rb.linearVelocity = new Vector2(dir * knockbackForce, rb.linearVelocity.y);

            yield return null;
        }

        isKnockedBack = false;

        currentState = IsGrounded ? PlayerState.Idle : PlayerState.Jump;
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