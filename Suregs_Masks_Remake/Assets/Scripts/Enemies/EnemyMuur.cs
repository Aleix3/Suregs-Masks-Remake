using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMuur : Enemy
{
    public Collider2D attackHitbox;
    public float dashDistance;
    public float dashForce = 10f;
    public float dashCooldown;
    private float dashCooldownTimer = 0f;
    public Color viewSecondColor = Color.magenta;
    float distance;

    public float stunTime;
    private float stunTimer;

    private bool isDashing = false;

    


    protected override void Start()
    {
        base.Start();
        attackHitbox.enabled = false;
        dashCooldownTimer = 0f;
        stunTimer = stunTime;
    }


    protected override void ExtraUpdate()
    {
        distance = Vector2.Distance(transform.position, player.position);

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        if (distance < viewDistance && distance > dashDistance && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            Dash();
        }

        if (isDashing)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f)
            {
                isDashing = false;
                stunTimer = stunTime;
            }
        }
    }
    protected override void Attack()
    {
        if (!canAttack) return;


        if (distance < dashDistance)
        {
            rb.velocity = Vector2.zero;
            //animator.Play("Attack");

            StartCoroutine(DoAttack());
        }
        else
        {
            
        }
        
    }

    protected override void Chase()
    {
        if (isDashing) return;
        base.Chase();          
    }
    void Dash()
    {

        print("daash");
        Vector2 direction = (player.position - transform.position).normalized;
        rb.AddForce(direction * dashForce, ForceMode2D.Impulse);
        dashCooldownTimer = dashCooldown;
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
