using System.Collections;
using UnityEngine;

public class EnemyOsiris : Enemy
{
    [Header("Osiris Attack")]
    public Collider2D attackHitbox;

    private bool hasRevived = false;
    private bool isReviving = false;
    public float reviveTime;
    private bool firstDeathDone = false;
    private bool isAttackingAnimation = false;

    protected override void Start()
    {
        base.Start();
        attackHitbox.enabled = false;
    }

    protected override void Update()
    {
        if (!isNotAttacking || isReviving)
        {
            animator.SetBool("isRunning", false);
            return;
        }

        base.Update();

        animator.SetBool("isRunning",
            desiredState == EnemyState.Running);
    }

    protected override void Attack()
    {
        // SI YA ESTÁ ATACANDO NO HACER NADA
        if (!isNotAttacking) return;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        agent.updatePosition = false;
        agent.updateRotation = false;

        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        isNotAttacking = false;
        canMove = false;

        isAttackingAnimation = true;

        agent.isStopped = true;

        agent.ResetPath();
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;

        animator.SetBool("isRunning", false);
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.1f);

        attackHitbox.enabled = true;

        yield return new WaitForSeconds(0.2f);

        attackHitbox.enabled = false;

        // Esperar a que termine la animación
        yield return new WaitForSeconds(0.5f);

        isAttackingAnimation = false;

        yield return new WaitForSeconds(attackCooldown);

        agent.updatePosition = true;
        agent.isStopped = false;

        canMove = true;
        isNotAttacking = true;
    }

    protected override void Die()
    {
        if (isReviving) return;

        rb.velocity = Vector2.zero;

        // Primera muerte
        if (!hasRevived && !firstDeathDone)
        {
            firstDeathDone = true;

            animator.SetTrigger("FirstDeath");

            StartCoroutine(ReviveCoroutine());
            return;
        }

        // Muerte final
        isDead = true;
        base.Die();
    }

    private IEnumerator ReviveCoroutine()
    {
        isReviving = true;

        agent.isStopped = true;

        yield return new WaitForSeconds(reviveTime);

        animator.SetTrigger("Revive");

        yield return new WaitForSeconds(1f);

        health = maxHealth;
        hasRevived = true;
        isReviving = false;

        agent.isStopped = false;

        desiredState = EnemyState.Idle;
    }

    public override void TakeDamage(int damage)
    {
        // Si está reviviendo en el suelo, muerte final
        if (isReviving)
        {
            hasRevived = true;
            isDead = true;

            StopAllCoroutines();

            base.Die();
            return;
        }

        base.TakeDamage(damage);
    }
}
