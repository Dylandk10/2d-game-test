using UnityEngine;
using static UnityEditor.PlayerSettings;


public class PlayerAnimation : MonoBehaviour
{

    [Header("Stats")]
    public PlayerStats stats;


    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
    [SerializeField] private string portalPoolKey = "Portal";
    [SerializeField] private Vector3 portalOffset = new Vector3(0f, -0.5f, 0f);

    //members
    private int facingDirection = 1;

    private PlayerMovement movement;
    private Animator animator;
    private Rigidbody2D rb;

    void Awake() 
    {
        movement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        float speed = Mathf.Abs(movement.MoveInput);

        animator.SetInteger(AnimStateHash, speed > 0 ? 1 : 0);

        animator.SetBool("IsGrounded", movement.IsGrounded);

        animator.SetFloat("AirSpeedY", rb.linearVelocity.y);

        //handle flip
        if (movement.MoveInput > 0)
            facingDirection = 1;
        else if (movement.MoveInput < 0)
            facingDirection = -1;
        transform.localScale = new Vector3(facingDirection, 1f, 1f);
    }


    //function called from player movement to set the trigger for the attack type aka Attack1, Attack2, Attack3
    public void UpdateAttack(string selected)
    {
        animator.SetTrigger(selected);
    }

    public void UpdateDash()
    {
        animator.SetTrigger("Roll");
    }
    public void UpdateHurt()
    {
        animator.SetTrigger("Hurt");
    }

    public void UpdateJump()
    {
        animator.SetTrigger("Jump");
    }

    public void PlayDeath()
    {
        animator.SetTrigger("Death");
    }
    public void OnDeathAnimationFinished()
    {
        Time.timeScale = 0f;

        // SHOW DEATH MENU HERE
        UIGameManager.Instance.ShowDeathMenu();
    }

    public void DealDamageHalfWayThroughAnimation()
    {
        Player.Instance.DealDamage();
        DisableDamage();
    }

    //for first swing in animation
    public void DealDamageFirstHit()
    {
        Player.Instance.ClearHitEnemies();
        Player.Instance.DealDamage();
    }

    //for second swing in animation
    public void DealDamageSecondHit()
    {
        Player.Instance.ClearHitEnemies();
        Player.Instance.DealDamage();
    }

    public void EnableDamage()
    {
        movement.SetCanDealDamage(true);
    }

    public void DisableDamage()
    {
        movement.SetCanDealDamage(false);
    }

    public void EndAttack()
    {
        if (movement.GetAttackRequest())
        {
            movement.SetAttackRequest(false);
            movement.StartAttack(); // immediately chain next attack
            return;
        }
        movement.EndAttack();
    }

    public void EndHurt()
    {
        movement.SetCanDealDamage(false);
        movement.SetAttackRequest(false);   

        movement.currentState = movement.IsGrounded
            ? PlayerMovement.PlayerState.Idle
            : PlayerMovement.PlayerState.Jump;
    }

    public void MoveDash()
    {
        SpawnStartPortal();
        movement.StartMoveDash(); 
    }

    public void EndDash()
    {
        movement.EndDashMovement();
        SpawnEndPortal();
    }

    private void SpawnStartPortal()
    {
        Debug.Log("SpawnStartPortal CALLED");
        GameObject obj = PoolManager.Instance.Get(portalPoolKey);
        PlayerPortal portal = obj.GetComponent<PlayerPortal>();

        bool facingRight = transform.localScale.x > 0;
        Vector3 pos;
        if (facingRight)
            pos = transform.position + new Vector3(-1.2f, 0.2f, 0f);
        else
            pos = transform.position + new Vector3(1.2f, 0.2f, 0f);

        portal.Activate(pos, facingRight);
    }

    private void SpawnEndPortal()
    {
        GameObject obj = PoolManager.Instance.Get(portalPoolKey);
        PlayerPortal portal = obj.GetComponent<PlayerPortal>();

        bool facingRight = transform.localScale.x > 0;
        Vector3 pos;
        if (facingRight)
            pos = transform.position + new Vector3(-1.2f, 0.2f, 0f);
        else
            pos = transform.position + new Vector3(1.2f, 0.2f, 0f);

        portal.Activate(pos, facingRight);
    }

}
