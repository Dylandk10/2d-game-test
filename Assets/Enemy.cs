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

    [Header("Attack")]
    public float attackCooldown = 1.5f;

    private float lastAttackTime;

    [Header("Hitbox")]
    public GameObject hitbox; // assign child object with Trigger collider

    void Start()
    {
        if (hitbox != null)
            hitbox.SetActive(false);
    }

    void Update()
    {
        if (PlayerInSight())
        {
            if (PlayerInAttackRange())
            {
                Attack();
            }
            else
            {
                MoveTowardPlayer();
            }
        }
    }

    // -------------------------
    // SIGHT (can detect player)
    // -------------------------
    bool PlayerInSight()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        return dx <= sightRangeX && dy <= sightRangeY;
    }

    // -------------------------
    // ATTACK RANGE (looser than before)
    // -------------------------
    bool PlayerInAttackRange()
    {
        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);

        float attackRangeX = 1.0f;
        float attackRangeY = 1.2f; // IMPORTANT: not too strict

        return dx <= attackRangeX && dy <= attackRangeY;
    }

    // -------------------------
    // MOVE ONLY ON X AXIS
    // -------------------------
    void MoveTowardPlayer()
    {
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        float newX = transform.position.x + direction * moveSpeed * Time.deltaTime;

        transform.position = new Vector3(
            newX,
            transform.position.y,
            0f
        );
    }

    // -------------------------
    // ATTACK (Hollow Knight style hitbox window)
    // -------------------------
    void Attack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        // Enable hitbox during attack window
        if (hitbox != null)
            hitbox.SetActive(true);

        // small attack timing window (tweak for feel)
        yield return new WaitForSeconds(0.15f);

        if (hitbox != null)
            hitbox.SetActive(false);
    }
}