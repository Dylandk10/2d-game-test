using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    //main components for subscripts
    public Rigidbody2D rb;
    public PlayerAnimation playerAnimatorScript;


    //privates
    private int lives = 6;
    private readonly int maxLives = 6;
    private int baseDamage = 50;
    private int damageBoost = 0;


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
        lives--;

        if (lives <= 0)
        {
            lives = 0;
            Die();
        }
        else
        {
            playerAnimatorScript.UpdateHurt();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");

        // Stop movement
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        // Optional: disable this script or input handling
        // enabled = false;

        // Play death animation
        playerAnimatorScript.PlayDeath();
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
}
