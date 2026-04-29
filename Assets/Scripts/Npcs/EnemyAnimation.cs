using Unity.VisualScripting;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
    private Animator animator;
    private Enemy enemyScript;
    private bool isDead = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        enemyScript = GetComponent<Enemy>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        animator.SetInteger(AnimStateHash, enemyScript.AnimState);
      
    }

    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    public void Death(bool didDie)
    {
        isDead = didDie;
        animator.SetTrigger("Hurt");
    }

    public void UpdateHurt()
    {
        animator.SetTrigger("Hurt");
    }

    public void OnLastFrameHurt()
    {
        if(isDead)
            animator.SetTrigger("Death");
    }
}
