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

    public AudioClip attackClip;
    public AudioClip dieClip;


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


        if (!isDashing && distance < dashDistance && distance > attackDistance && dashCooldownTimer <= 0f)
        {
            StartCoroutine(DashCoroutine());
        }
        else
        {
            bool isRunning =
            agent.velocity.magnitude > 0.1f &&
            !agent.isStopped &&
            isNotAttacking && canMove;

            animator.SetBool("IsRunning", isRunning);
        }

        

        //if (isDashing)
        //{
        //    stunTimer -= Time.deltaTime;

        //    if (stunTimer <= 0f)
        //    {
        //        isDashing = false;

        //        animator.SetBool("isDashing", false);
        //        animator.SetBool("isStunned", true);

        //        StartCoroutine(StunCoroutine());
        //    }
        //}

        //animator.SetFloat("Speed", rb.velocity.magnitude);

    }
    protected override void Attack()
    {
        if (!isNotAttacking || isDashing) return;


        if (distance < attackDistance)
        {
            rb.velocity = Vector2.zero;
            animator.SetTrigger("Attack");

            StartCoroutine(DoAttack());
        }
        else
        {
            
        }


        
    }

    protected override void Chase()
    {
        if (isDashing || !canMove) return;
        
        base.Chase();          
    }

    IEnumerator DashCoroutine()
    {
        isDashing = true;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();

        canMove = false;

        animator.SetTrigger("Dash");
        AudioManager.Instance.PlaySFX(attackClip);
        attackHitbox.enabled = true;
        Vector2 direction = (player.position - transform.position).normalized;

        float dashDuration = 1f;
        float timer = 0f;

        while (timer < dashDuration)
        {
            rb.velocity = direction * dashForce;

            timer += Time.deltaTime;

            yield return null;
        }

        // desaceleración suave
        float slowDownTime = 0.2f;
        float currentSpeed = dashForce;

        animator.SetTrigger("SlowDown");
        

        while (currentSpeed > 0f)
        {
            currentSpeed -= dashForce * Time.deltaTime / slowDownTime;

            rb.velocity = direction * currentSpeed;

            yield return null;
        }

        rb.velocity = Vector2.zero;
        attackHitbox.enabled = false;
        animator.SetBool("IsStunned", true);

        yield return new WaitForSeconds(stunTime);

        animator.SetBool("IsStunned", false);

        dashCooldownTimer = dashCooldown;

        canMove = true;
        isDashing = false;

        agent.isStopped = false;
    }

    private IEnumerator DoAttack()
    {
        isNotAttacking = false;
        canMove = false;

        yield return new WaitForSeconds(0.1f);
        AudioManager.Instance.PlaySFX(attackClip);
        attackHitbox.enabled = true;

        yield return new WaitForSeconds(0.2f);
        attackHitbox.enabled = false;

        yield return new WaitForSeconds(attackCooldown);
        isNotAttacking = true;
        canMove = true;
    }

    IEnumerator StunCoroutine()
    {
        canMove = false;

        yield return new WaitForSeconds(stunTime);

        animator.SetBool("IsStunned", false);

        canMove = true;
    }

    protected override void Die()
    {
        if (isDead) return;

        //isDead = true;

        animator.SetTrigger("Die");

        rb.velocity = Vector2.zero;

        AudioManager.Instance.PlaySFX(dieClip);
        StartCoroutine(DieCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        yield return new WaitForSeconds(1f);

        base.Die();
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // dibuja radio base

        // Gizmos exclusivos del hijo
        Gizmos.color = viewSecondColor;
        Gizmos.DrawWireSphere(transform.position, dashDistance);
    }

}
