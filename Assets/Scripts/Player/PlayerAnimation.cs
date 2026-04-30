using UnityEngine;


public class PlayerAnimation : MonoBehaviour
{
    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");

    //members
    private int facingDirection = 1;

    private PlayerMovement movement;
    private Animator animator;

    void Start() 
    {
        movement = Player.Instance.GetPlayerMovement();
        animator = Player.Instance.GetAnimator();
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

        animator.SetFloat("AirSpeedY", movement.Velocity.y);

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

}
