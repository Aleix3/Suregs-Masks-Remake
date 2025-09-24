using System.Collections;
using UnityEngine;

public class EnemyOsiris : Enemy
{
    [Header("Osiris Attack")]
    public Collider2D attackHitbox;

    private bool hasRevived = false;
    private bool isReviving = false;
    public float reviveTime;

    protected override void Start()
    {
        base.Start();
        attackHitbox.enabled = false;
    }

    protected override void Attack()
    {
        if (!canAttack) return;

        rb.velocity = Vector2.zero;
        //animator.Play("Attack");

        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        canAttack = false;

        yield return new WaitForSeconds(0.1f);
        attackHitbox.enabled = true;

        yield return new WaitForSeconds(0.2f);
        attackHitbox.enabled = false;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    protected override void Die()
    {
        rb.velocity = Vector2.zero;
        //animator.Play("Die");

        if (!hasRevived)
        {
            StartCoroutine(ReviveCoroutine());
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }

    private IEnumerator ReviveCoroutine()
    {
        isReviving = true;
        //animator.Play("Revive");

        yield return new WaitForSeconds(reviveTime);

        health = maxHealth;
        hasRevived = true;
        isReviving = false;
        desiredState = EnemyState.Idle;
    }
}
