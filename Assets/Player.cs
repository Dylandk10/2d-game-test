using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    //main components for subscripts
    public Rigidbody2D rb;
    public PlayerAnimation playerAnimatorScript;


    //privates
    private int health = 100;
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

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        playerAnimatorScript.UpdateHurt();
    }

    public int GetDamage()
    {
        return damageBoost + baseDamage;
    }
}
