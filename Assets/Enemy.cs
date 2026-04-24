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
    public float attackRangeX = 3f;

    [Header("Attack")]
    public float attackCooldown = 1.5f;

    private float lastAttackTime;

    [Header("Hitbox")]
    public GameObject hitbox;

    private bool facingRight = false;
    private SpriteRenderer spriteRenderer;

    public Transform hitboxPoint;
    public Vector3 rightOffset;
    public Vector3 leftOffset;
    public Rigidbody2D rb;

    [SerializeField] private LayerMask playerLayer;
    private Collider2D[] results = new Collider2D[4]; // reuse buffer

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        if (hitbox != null)
            hitbox.SetActive(false);

        UpdateHitboxPosition();
    }

    void Update()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        bool inAttackRange = dx <= attackRangeX;
        bool inSight = PlayerInSight(dx, dy);

        if (!inSight)
            return;

        if (inAttackRange)
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
        else
        {
            MoveTowardPlayer(dx);
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
    void MoveTowardPlayer(float dx)
    {
        float direction = player.position.x >= transform.position.x ? 1f : -1f;

        // Flip sprite logic
        if (direction > 0 && !facingRight)
            Flip();
        else if (direction < 0 && facingRight)
            Flip();

        Vector2 newPosition = rb.position + new Vector2(direction * moveSpeed * Time.fixedDeltaTime, 0f);
        rb.MovePosition(newPosition);
    }

    void Flip()
    {
        facingRight = !facingRight;

        spriteRenderer.flipX = facingRight;

        UpdateHitboxPosition();
    }

    void UpdateHitboxPosition()
    {
        if (hitboxPoint == null) return;

        hitboxPoint.localPosition = facingRight ? rightOffset : leftOffset;
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        if (hitbox != null)
        {
            UpdateHitboxPosition();
            hitbox.SetActive(true);
            Physics2D.SyncTransforms();
            CheckHitboxNow();
        }

        yield return new WaitForSeconds(0.15f);

        if (hitbox != null)
            hitbox.SetActive(false);
    }

    void CheckHitboxNow()
    {
        Collider2D col = hitbox.GetComponent<Collider2D>();

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(playerLayer);
        filter.useTriggers = true;

        int count = col.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            Debug.Log("Player hit (manual check)!");
            Debug.Log(Time.time);

            // If you only want ONE hit per attack, stop here:
            return;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(sightRangeX * 2f, sightRangeY * 2f, 0f));

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(attackRangeX * 2f, 1f, 0f));
    }
}