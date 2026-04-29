using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Sight Range")]
    public float sightRangeX = 7f;
    public float sightRangeY = 2f;
    public float attackRangeX = 1.4f;

    [Header("Attack")]
    [SerializeField] float attackDelay = 0.4f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("Death")]
    [SerializeField] private float deathDestroyDelay = 1.2f;

    [Header("Patrol")]
    public float patrolRange = 4f;
    public float patrolSpeed = 1.5f;
    public float idleTime = 2f;
    public float patrolTime = 3f;
    [Range(0f, 1f)] public float patrolChance = 0.6f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.2f;

    private Vector2 startPosition;
    private float stateTimer;
    private bool isPatrolling = false;
    private float patrolDirection = -1f; // start facing left

    private float currentMoveDirection = 0f;
    private float currentMoveSpeed = 0f;

    private int facingDirection = 1;

    private EnemyState currentState;

    private bool isAttacking = false;
    private bool isDead = false;

    public Rigidbody2D rb;
    private EnemyAnimation animScript;

    private int health = 100;

    public int AnimState = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animScript = GetComponent<EnemyAnimation>();

        currentState = EnemyState.Idle;
        startPosition = transform.position;
        stateTimer = idleTime;
    }

    void Update()
    {
        if (currentState == EnemyState.Dead) return;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        bool inSight = PlayerInSight(dx, dy);
        bool inAttackRange = dx <= attackRangeX;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (isPatrolling)
                    HandlePatrol(inSight, inAttackRange);
                else
                    HandleIdle(inSight, inAttackRange);
                break;

            case EnemyState.Chase:
                HandleChase(inSight, inAttackRange);
                break;

            case EnemyState.Attack:
                HandleAttack(inSight, inAttackRange);
                break;
        }
    }

    void FixedUpdate()
    {
        if (currentState == EnemyState.Dead) return;

        rb.linearVelocity = new Vector2(
            currentMoveDirection * currentMoveSpeed,
            rb.linearVelocity.y
        );

        currentMoveDirection = 0f;
    }

    // -------------------------
    // STATES
    // -------------------------

    void HandleIdle(bool inSight, bool inAttackRange)
    {
        AnimState = 0;

        if (inSight)
        {
            currentState = inAttackRange ? EnemyState.Attack : EnemyState.Chase;
            return;
        }

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0)
        {
            if (Random.value < patrolChance)
                StartPatrol();
            else
                stateTimer = idleTime;
        }
    }

    void HandlePatrol(bool inSight, bool inAttackRange)
    {
        if (inSight)
        {
            isPatrolling = false;
            currentState = inAttackRange ? EnemyState.Attack : EnemyState.Chase;
            return;
        }

        float dir = patrolDirection;

        bool canForward = CanMove(dir);
        bool canBackward = CanMove(-dir);

        // -------------------------
        // Both directions blocked
        // -------------------------
        if (!canForward && !canBackward)
        {
            AnimState = 0;
            stateTimer = idleTime;
            isPatrolling = false;
            currentState = EnemyState.Idle;
            return;
        }

        // -------------------------
        // Try forward
        // -------------------------
        if (canForward)
        {
            patrolDirection = dir;
        }
        else if (canBackward)
        {
            patrolDirection = -dir;
        }

        // -------------------------
        // Move
        // -------------------------
        AnimState = 2;
        Flip(patrolDirection);
        Move(patrolDirection, patrolSpeed);

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            isPatrolling = false;
            stateTimer = idleTime;
        }
    }

    void HandleChase(bool inSight, bool inAttackRange)
    {
        float direction = player.position.x >= transform.position.x ? 1f : -1f;
        if (CanMove(direction))
            AnimState = 2;
        else
            AnimState = 0;

        if (!CanMove(direction))
        {
            Flip(direction);
            return;
        }

        Flip(direction);
        Move(direction, moveSpeed);

        if (!inSight)
        {
            currentState = EnemyState.Idle;
            return;
        }

        if (inAttackRange)
            currentState = EnemyState.Attack;
    }

    void HandleAttack(bool inSight, bool inAttackRange)
    {
        AnimState = 0;

        if (!inAttackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
            Attack();

        if (!inSight)
            currentState = EnemyState.Idle;
    }

    void StartPatrol()
    {
        isPatrolling = true;
        stateTimer = patrolTime;

        patrolDirection = Random.value < 0.5f ? -1f : 1f;

        if (!CanMove(patrolDirection))
            patrolDirection *= -1f;
    }

    // -------------------------
    // MOVEMENT
    // -------------------------

    void Move(float direction, float speed)
    {
        currentMoveDirection = direction;
        currentMoveSpeed = speed;
    }

    void Flip(float direction)
    {
        if (direction == 0) return;

        facingDirection = (int)direction;
        transform.localScale = new Vector3(facingDirection, 1f, 1f);
    }

    // -------------------------
    // ENVIRONMENT CHECKS
    // -------------------------

    bool CanMove(float direction)
    {
        return !IsWallAhead(direction) && IsGroundAhead(direction);
    }

    bool IsWallAhead(float direction)
    {
        Vector2 origin = wallCheck.position;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * direction,
            wallCheckDistance,
            groundLayer
        );

        // optional debug
        Debug.DrawRay(origin, Vector2.right * direction * wallCheckDistance, Color.red);

        if (hit.collider == null)
            return false;

        // small tolerance prevents edge jitter on composite colliders
        return hit.distance <= wallCheckDistance;
    }

    bool IsGroundAhead(float direction)
    {
        Vector2 origin = (Vector2)groundCheck.position + Vector2.right * direction * 0.05f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    // -------------------------
    // ATTACK
    // -------------------------

    void Attack()
    {
        if (!isAttacking)
            StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        AnimState = 0;
        animScript.Attack();

        yield return new WaitForSeconds(attackDelay);

        if (CheckPlayerInAttackRange())
            CombatManager.Instance.TryHitPlayer();

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    bool CheckPlayerInAttackRange()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        return dx <= attackRangeX;
    }

    bool PlayerInSight(float dx, float dy)
    {
        return dx <= sightRangeX && dy <= sightRangeY;
    }

    // -------------------------
    // DAMAGE / DEATH
    // -------------------------

    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead) return;

        health -= damage;

        if (health <= 0)
            Die();
        else
            animScript.UpdateHurt();
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = EnemyState.Dead;

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        GetComponent<Collider2D>().enabled = false;

        animScript.UpdateHurt();
        animScript.Death();

        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(deathDestroyDelay);
        Destroy(gameObject);
    }
}