using System.Collections;
using UnityEngine;

public class EnemyWarrior : Enemy
{
    [Header("Warrior Attack")]
    public Collider2D attackHitbox;

    private bool hasRevived = false;
    private bool isReviving = false;
    public float reviveTime;
    private bool firstDeathDone = false;
    private bool isAttackingAnimation = false;
    private bool finalKill = true;

    protected override void Start()
    {
        base.Start();
        attackHitbox.enabled = false;
    }

    protected override void Update()
    {

        base.Update();



    }

    protected override void Attack()
    {
        if (!isNotAttacking) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        agent.updatePosition = false;
        agent.updateRotation = false;

        DoAttack();
    }

    private void DoAttack()
    {
        isNotAttacking = false;
        canMove = false;

        isAttackingAnimation = true;

        agent.isStopped = true;

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;


        animator.SetTrigger("Attack");

        //yield return new WaitForSeconds(0.1f);

        
    }

    public IEnumerator TriggerHitBox()
    {
        attackHitbox.enabled = true;

        yield return new WaitForSeconds(0.05f);

        attackHitbox.enabled = false;

        //// Esperar a que termine la animación
        //yield return new WaitForSeconds(0.2f);

        isAttackingAnimation = false;

        yield return new WaitForSeconds(attackCooldown);

        agent.updatePosition = true;
        agent.isStopped = false;

        canMove = true;
        isNotAttacking = true;
    }

    protected override void Die()
    {
        //if (isReviving) return;

        rb.velocity = Vector2.zero;

        // Primera muerte
        if (!hasRevived && !firstDeathDone)
        {
            canMove = false;
            firstDeathDone = true;
            agent.isStopped = true;
            health = 1;

            animator.SetTrigger("FirstDeath");

            StartCoroutine(ReviveCoroutine());
            return;
        }


        base.Die();
    }

    private IEnumerator ReviveCoroutine()
    {
        
        agent.isStopped = true;
        desiredState = EnemyState.Idle;

        isReviving = true;
        yield return new WaitForSeconds(reviveTime);
        
        animator.SetTrigger("Revive");

        //yield return new WaitForSeconds(1f);

        health = maxHealth;
        hasRevived = true;
        isReviving = false;
        canMove = true;
        agent.isStopped = false;

        desiredState = EnemyState.Idle;
    }

}
