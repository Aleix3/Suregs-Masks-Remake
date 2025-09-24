using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMuur : Enemy
{
    public Collider2D attackHitbox;
    public float dashDistance;
    public float dashForce = 10f;
    public Color viewSecondColor = Color.magenta;
    
    protected override void Start()
    {
        base.Start();
        attackHitbox.enabled = false;
    }
    protected override void Attack()
    {
        if (!canAttack) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < dashDistance)
        {
            rb.velocity = Vector2.zero;
            //animator.Play("Attack");

            StartCoroutine(DoAttack());
        }
        else
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.AddForce(direction * dashForce, ForceMode2D.Impulse);
            //isDashing = true;
            //dashTimer = dashDuration;
            //dashCooldownTimer = dashCooldown;
        }
        
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
        Destroy(gameObject, 1f);

    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // dibuja radio base

        // Gizmos exclusivos del hijo
        Gizmos.color = viewSecondColor;
        Gizmos.DrawWireSphere(transform.position, dashDistance);
    }

}
