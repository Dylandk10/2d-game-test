using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
    private Animator animator;
    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;


    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
            spriteRenderer.flipX = false;
        else if (movement.MoveInput < 0)
            spriteRenderer.flipX = true;
    }

    public void UpdateAttack(string selected)
    {
        animator.SetTrigger(selected);
    }
}
