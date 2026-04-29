using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    //main components for subscripts
    public Rigidbody2D rb;
    public PlayerAnimation playerAnimatorScript;
    public PlayerMovement playerMovement;
    public Animator animator;
    private CapsuleCollider2D capsuleCollider;
    private SpriteRenderer spriteRenderer;

    // for attacks
    [SerializeField] public Transform attackPoint;
    [SerializeField] private Vector2 attackSize = new Vector2(1.5f, 1f);
    [SerializeField] private LayerMask enemyLayer;
    private HashSet<Enemy> hitEnemies = new HashSet<Enemy>();


    //privates
    private int lives = 6;
    private readonly int maxLives = 6;
    private int baseDamage = 50;
    private int damageBoost = 0;
    private bool isInvincible = false;
    [SerializeField] private float invincibleDuration = 1.0f;




    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional: persist between scenes
        DontDestroyOnLoad(gameObject);

        // for the components of player
        rb = GetComponent<Rigidbody2D>();
        playerAnimatorScript = GetComponent<PlayerAnimation>();
        playerMovement = GetComponent<PlayerMovement>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void TakeDamage()
    {
        if (isInvincible) return; // if already hit
        playerMovement.currentState = PlayerMovement.PlayerState.Hurt;
        lives--;

        if (lives <= 0)
        {
            StopAllCoroutines();
            lives = 0;
            Die();
        }
        playerMovement.EndAttack();
        StartCoroutine(HitStop(0.2f));
        StartCoroutine(InvincibilityRoutine());
        playerAnimatorScript.UpdateHurt();
    }

    IEnumerator HitStop(float duration)
    {
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        float timer = 0f;

        while (timer < invincibleDuration)
        {
            // 🔥 bright flash
            spriteRenderer.color = new Color(2f, 2f, 2f, 1f);
            yield return new WaitForSeconds(0.05f);

            // back to normal
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.05f);

            timer += 0.1f;
        }


        spriteRenderer.color = Color.white;
        isInvincible = false;
    }

    private void Die()
    {
        capsuleCollider.enabled = false;
        // Stop movement
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Optional: disable this script or input handling
        // enabled = false;

        // Play death animation
        playerAnimatorScript.PlayDeath();
    }

    public void DealDamage()
    {
        if (!playerMovement.GetCanDealDamage()) return;

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
                enemy.TakeDamage(GetDamage());
            }
        }
    }

    public void ClearHitEnemies()
    {
        hitEnemies.Clear();
    }


    public int GetDamage()
    {
        return damageBoost + baseDamage;
    }
    public int GetLives()
    {
        return lives;
    }
    public int GetMaxLives() {
        return maxLives;
    }

    // debug
    void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, attackSize);
        }
    }
}
