using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 2f;

    [Header("Sight Range (when enemy notices player)")]
    public float sightRangeX = 7f;
    public float sightRangeY = 2f;
    public float attackRangeX = 1.4f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;

    private float lastAttackTime;

    [Header("Hitbox")]
    public GameObject hitbox;

    private bool facingRight = false;
    private SpriteRenderer spriteRenderer;

    public Rigidbody2D rb;

    [SerializeField] private LayerMask playerLayer;
    private Collider2D[] results = new Collider2D[4]; // reuse buffer

    // Animation state
    public int AnimState = 0;
    private EnemyAnimation animScript;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animScript = GetComponent<EnemyAnimation>();
    }

    void Update()
    {
        float playerDirection = player.position.x >= transform.position.x ? 1f : -1f;
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        bool inAttackRange = dx <= attackRangeX;
        bool inSight = PlayerInSight(dx, dy);

        if (!inSight)
        {
            AnimState = 0;
            return;
        }

        Flip(playerDirection);

        if (inAttackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
        else
        {
            MoveTowardPlayer(dx, playerDirection);
        }
    }

    // -------------------------
    // SIGHT (can detect player)
    // -------------------------
    bool PlayerInSight(float dx, float dy)
    {
        return dx <= sightRangeX && dy <= sightRangeY;
    }



    // -------------------------
    // MOVE ONLY ON X AXIS
    // -------------------------
    void MoveTowardPlayer(float dx, float direction)
    {

        Vector2 newPosition = rb.position + new Vector2(direction * moveSpeed * Time.fixedDeltaTime, 0f);
        rb.MovePosition(newPosition);
        AnimState = 2;
    }

    void Flip(float direction)
    {
        // Flip sprite logic
        if (direction > 0 && !facingRight)
            facingRight = true;
        else if (direction < 0 && facingRight)
            facingRight = false;

        spriteRenderer.flipX = facingRight;
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        AnimState = 0;
        animScript.Attack();
        CombatManager.Instance.TryHitPlayer(10);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(sightRangeX * 2f, sightRangeY * 2f, 0f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRangeX * 2f, 1f, 0f));
    }
}