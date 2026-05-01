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


    private float lastDashTime;
    private bool dashRequested;
    private int facingDirection = 1;

    [Header("Attack")]
    private bool canAttack = true;
    private bool canDealDamage = true;
    private bool attackRequested = false;

    public float MoveInput;
    private Rigidbody2D rb;
    private PlayerAnimation playerAnimationScript;

    [Header("Ghost Trail")]
    public GhostPool ghostPool;
    public float ghostDistanceStep = 0.05f; // lower = more ghosts
    public Color ghostColor = new Color(1f, 1f, 1f, 0.5f);

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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if(jumpCount < 1)
                playerAnimationScript.UpdateJump();
            else
                playerAnimationScript.UpdateDash();
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

        currentState = PlayerState.Dash;

        float dir = MoveInput != 0 ? MoveInput : facingDirection;

        playerAnimationScript.UpdateDash();
        StartCoroutine(DashRoutine(dir));
    }

    IEnumerator DashRoutine(float direction)
    {
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float maxDistance = dashDistance;

        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.right * direction, dashDistance, groundLayer);

        if (hit)
        {
            maxDistance = hit.distance;
        }

        float dashTime = maxDistance / dashSpeed;
        StartCoroutine(SpawnGhostsDuringDash());

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        yield return new WaitForSeconds(dashTime);

        rb.linearVelocity = Vector2.zero;

        rb.gravityScale = originalGravity;
        currentState = PlayerState.Move;
    }


    /// <Dashghost>
    /// Used for spawning a lingering ghost behind on player dash need to turn into pool
    /// </Dashghost>
    /// 
    IEnumerator SpawnGhostsDuringDash()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        Vector3 lastPosition = transform.position;

        while (currentState == PlayerState.Dash)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);

            if (distanceMoved >= ghostDistanceStep)
            {
                SpawnGhost(sr);
                lastPosition = transform.position;
            }

            yield return null;
        }
    }

    void SpawnGhost(SpriteRenderer playerSR)
    {
        Ghost ghost = ghostPool.GetGhost();

        ghost.transform.position = transform.position;

        ghost.Init(
            playerSR.sprite,
            playerSR.flipX,
            ghostColor,
            playerSR.sortingOrder - 1,
            transform.localScale,
            ghostPool
        );
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

        playerAnimationScript.UpdateAttack(selected);
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