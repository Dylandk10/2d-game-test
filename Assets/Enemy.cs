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

    [Header("Hitbox")]
    public GameObject hitbox;

    [Header("Death")]
    [SerializeField] private float deathDestroyDelay = 1.2f;

    private EnemyState currentState;

    private bool facingRight = false;
    private bool isAttacking = false;
    private bool isDead = false;

    private SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;

    private Collider2D[] results = new Collider2D[4];

    public int AnimState = 0;
    private EnemyAnimation animScript;

    private int health = 100;
    private int damage = 10;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animScript = GetComponent<EnemyAnimation>();

        currentState = EnemyState.Idle;
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
                HandleIdle(inSight, inAttackRange);
                break;

            case EnemyState.Chase:
                HandleChase(inSight, inAttackRange, dx);
                break;

            case EnemyState.Attack:
                HandleAttack(inSight, inAttackRange);
                break;
        }
    }

    // -------------------------
    // STATES
    // -------------------------

    void HandleIdle(bool inSight, bool inAttackRange)
    {
        AnimState = 0;

        if (!inSight) return;

        if (inAttackRange)
            currentState = EnemyState.Attack;
        else
            currentState = EnemyState.Chase;
    }

    void HandleChase(bool inSight, bool inAttackRange, float dx)
    {
        AnimState = 2;

        float direction = player.position.x >= transform.position.x ? 1f : -1f;

        Flip(direction);
        MoveTowardPlayer(direction);

        if (!inSight)
        {
            currentState = EnemyState.Idle;
            return;
        }

        if (inAttackRange)
        {
            currentState = EnemyState.Attack;
        }
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
        {
            Attack();
        }

        if (!inSight)
        {
            currentState = EnemyState.Idle;
        }
    }

    // -------------------------
    // MOVEMENT
    // -------------------------

    void MoveTowardPlayer(float direction)
    {
        Vector2 newPosition = rb.position + new Vector2(direction * moveSpeed * Time.fixedDeltaTime, 0f);

        rb.MovePosition(newPosition);
    }

    void Flip(float direction)
    {
        if (direction > 0 && !facingRight)
            facingRight = true;
        else if (direction < 0 && facingRight)
            facingRight = false;

        spriteRenderer.flipX = facingRight;
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
            CombatManager.Instance.TryHitPlayer(damage);

        lastAttackTime = Time.time;

        isAttacking = false;
    }

    bool CheckPlayerInAttackRange()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        return dx <= attackRangeX;
    }

    // -------------------------
    // SIGHT
    // -------------------------

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
        {
            Die();
        } else
        {
            animScript.UpdateHurt(); 
        }
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

    // -------------------------
    // DEBUG
    // -------------------------

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position,
            new Vector3(sightRangeX * 2f, sightRangeY * 2f, 0f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position,
            new Vector3(attackRangeX * 2f, 1f, 0f));
    }
}