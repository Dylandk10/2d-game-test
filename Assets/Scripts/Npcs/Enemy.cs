using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead,
        Hurt
    }

    [Header("Stats")]
    public EnemyStats stats;

    [Header("Player")]
    public Transform player;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;

    private Vector2 startPosition;
    private float stateTimer;
    private bool isPatrolling = false;
    private float patrolDirection = -1f;

    private float currentMoveDirection = 0f;
    private float currentMoveSpeed = 0f;

    private int facingDirection = 1;
    private EnemyState currentState;

    private bool isAttacking = false;
    private bool isDead = false;

    private float lastAttackTime;
    private int health;

    public Rigidbody2D rb;
    private EnemyAnimation animScript;

    public int AnimState = 0;

    private bool isHurt = false;
    private bool pendingDeath = false;
    private float blockedDirection = 0f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animScript = GetComponent<EnemyAnimation>();

        // Auto-find player if forget to assign not great with tag but will be a safe fall back
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentState = EnemyState.Idle;
        startPosition = transform.position;

        stateTimer = stats.idleTime;
        health = stats.maxHealth;
    }

    void Update()
    {
        if (currentState == EnemyState.Dead || player == null || currentState == EnemyState.Hurt) return;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        bool inSight = PlayerInSight(dx, dy);
        bool inAttackRange = dx <= stats.attackRangeX && dy <= stats.attackRangeY;

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

    public void Initialize(Transform playerTransform)
    {
        player = playerTransform;

        // Reset important gameplay state
        health = stats.maxHealth;
        isDead = false;
        isHurt = false;
        isAttacking = false;

        currentState = EnemyState.Idle;

        rb.bodyType = RigidbodyType2D.Dynamic;
        GetComponent<Collider2D>().enabled = true;
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
            if (Random.value < stats.patrolChance)
                StartPatrol();
            else
                stateTimer = stats.idleTime;
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

        HandleMovement(ref patrolDirection, stats.patrolSpeed, false);

        stateTimer -= Time.deltaTime;

        if (stateTimer <= 0f)
        {
            isPatrolling = false;
            stateTimer = stats.idleTime;
        }
    }

    void HandleChase(bool inSight, bool inAttackRange)
    {
        float direction = player.position.x >= transform.position.x ? 1f : -1f;

        HandleMovement(ref direction, stats.moveSpeed, true);

        if (!inSight)
        {
            currentState = EnemyState.Idle;
            return;
        }

        if (inAttackRange)
            currentState = EnemyState.Attack;
    }

    void HandleMovement(ref float direction, float baseSpeed, bool useForcedDirection)
    {
        if (blockedDirection != 0f)
        {
            direction = blockedDirection;
        }
        bool blockedForward = IsWallAhead(direction) || IsEnemyTooClose(direction);
        bool blockedBackward = IsWallAhead(-direction) || IsEnemyTooClose(-direction);

        // Fully stuck
        if (blockedForward && blockedBackward)
        {
            blockedDirection = -direction;
            Move(0, 0);
            AnimState = 0;
            return;
        }



        float speed = baseSpeed;

        if (IsEnemyAhead(direction))
            speed *= 0.4f;

        if (IsEnemyTooClose(direction))
            speed = 0f;

        if (IsWallAhead(direction) || !IsGroundAhead(direction))
            speed = 0f;

        AnimState = speed > 0 ? 2 : 0;

        if (speed > 0.01f)
            Flip(direction);

        Move(direction, speed);
        blockedDirection = 0f;
        return;
    }

    void HandleAttack(bool inSight, bool inAttackRange)
    {
        AnimState = 0;

        if (!inAttackRange)
        {
            currentState = EnemyState.Chase;
            return;
        }

        if (Time.time >= lastAttackTime + stats.attackCooldown)
            Attack();

        if (!inSight)
            currentState = EnemyState.Idle;
    }

    void StartPatrol()
    {
        isPatrolling = true;
        stateTimer = stats.patrolTime;

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
        return !IsWallAhead(direction) && IsGroundAhead(direction) && !IsEnemyAhead(direction);
    }

    bool IsWallAhead(float direction)
    {
        Vector2 origin = wallCheck.position;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * direction,
            stats.wallCheckDistance,
            groundLayer
        );

        return hit.collider != null && hit.distance <= stats.wallCheckDistance;
    }

    bool IsGroundAhead(float direction)
    {
        Vector2 origin = (Vector2)groundCheck.position + Vector2.right * direction * 0.05f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            stats.groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    bool IsEnemyAhead(float direction)
    {
        Vector2 origin = (Vector2)wallCheck.position + Vector2.right * direction * 0.2f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * direction,
            0.8f, // longer = awareness
            enemyLayer
        );

        return hit.collider != null && hit.collider.gameObject != gameObject;
    }

    bool IsEnemyTooClose(float direction)
    {
        Vector2 origin = (Vector2)wallCheck.position + Vector2.right * direction * 0.2f;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.right * direction,
            0.3f, // SHORT distance = "too close"
            enemyLayer
        );

        return hit.collider != null && hit.collider.gameObject != gameObject;
    }

    // -------------------------
    // ATTACK
    // -------------------------
    void Attack()
    {
        if (isAttacking || isHurt) return;

        isAttacking = true;
        animScript.Attack();
    }

    public bool CheckPlayerInAttackRange()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);
        return dx <= stats.attackRangeX && dy <= stats.attackRangeY;
    }

    bool PlayerInSight(float dx, float dy)
    {
        return dx <= stats.sightRangeX && dy <= stats.sightRangeY;
    }

    // -------------------------
    // DAMAGE / DEATH
    // -------------------------

    public void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead || isHurt) return;

        health -= damage;

        if (health <= 0)
        {
            pendingDeath = true;
        }

        EnterHurt();
    }

    void EnterHurt()
    {
        isHurt = true;
        currentState = EnemyState.Hurt;

        // Cancel attack immediately
        isAttacking = false;

        rb.linearVelocity = Vector2.zero;

        animScript.ForceHurt();
    }

    public void SetCurrentState(EnemyState enemyState)
    {
        currentState = enemyState;
    }

    public void SetIsHurt(bool hurt)
    {
        isHurt = hurt;
    }

    public void SetIsAttacking(bool attacking)
    {
        isAttacking = attacking;
    }

    public void SetLastAttackTime(float lastAttack)
    {
        lastAttackTime = lastAttack;
    }

    public bool GetPendingDeath()
    {
        return pendingDeath;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        currentState = EnemyState.Dead;

        StopAllCoroutines();
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;

        GetComponent<Collider2D>().enabled = false;

        animScript.Death();

        StartCoroutine(DestroyAfterDeath());
    }

    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(stats.deathDestroyDelay);
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        // Fallback in case Initialize wasn't called
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    //debug
    void OnDrawGizmosSelected()
    {
        if (stats == null) return;

        // 🔴 Attack range
        Gizmos.color = Color.red;

        Vector3 attackSize = new Vector3(
            stats.attackRangeX * 2f,
            stats.attackRangeY * 2f,
            1f
        );

        Gizmos.DrawWireCube(transform.position, attackSize);

        // 🟡 Sight range
        Gizmos.color = Color.yellow;

        Vector3 sightSize = new Vector3(
            stats.sightRangeX * 2f,
            stats.sightRangeY * 2f,
            1f
        );

        Gizmos.DrawWireCube(transform.position, sightSize);

        // 🟦 Wall check
        if (wallCheck != null)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawLine(
                wallCheck.position,
                wallCheck.position + Vector3.right * transform.localScale.x * stats.wallCheckDistance
            );
        }

        // 🟩 Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;

            Vector3 origin = groundCheck.position + Vector3.right * transform.localScale.x * 0.05f;

            Gizmos.DrawLine(
                origin,
                origin + Vector3.down * stats.groundCheckDistance
            );
        }
    }
}