using Unity.VisualScripting;
using UnityEngine;
using static Enemy;

public class EnemyAnimation : MonoBehaviour
{
    private static readonly int AnimStateHash = Animator.StringToHash("AnimState");
    private Animator animator;
    private Enemy enemyScript;

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

    //mid animation check hit
    public void OnAttackHit()
    {
        if (enemyScript.CheckPlayerInAttackRange())
            CombatManager.Instance.TryHitPlayer(transform.position);
    }

    //reset on last frame animation attack
    public void OnAttackFinished()
    {
        enemyScript.SetIsAttacking(false);
        enemyScript.SetLastAttackTime(Time.time);
    }

    public void Death()
    {
        animator.SetTrigger("Death");
    }

    public void UpdateHurt()
    {
        animator.SetTrigger("Hurt");
    }

    public void ForceHurt()
    {
        animator.ResetTrigger("Attack");      // cancel attack trigger
        animator.Play("Hurt", 0, 0f);         // HARD override current animation
    }

    public void OnLastFrameHurt()
    {
        enemyScript.SetIsHurt(false);
        if (enemyScript.GetPendingDeath())
        {
            enemyScript.Die();
            return;
        }
        enemyScript.SetCurrentState(EnemyState.Idle);
    }
}
