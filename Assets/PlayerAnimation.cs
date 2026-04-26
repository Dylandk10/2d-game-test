using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");

    // components
    private Animator animator;
    private PlayerMovement movement;

    //members
    private int facingDirection = 1;


    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();

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

}
