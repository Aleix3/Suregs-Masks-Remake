using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class EnemyOls : Enemy
{
    // Start is called before the first frame update
    public float escapeDistance;
    private bool escaping = false;
    public GameObject olsProjectile;
    public float projectileSpeed;
    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        if (player == null || isStunned || isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= escapeDistance && canMove)
        {
            escaping = true;
            desiredState = EnemyState.Running;
            StateMachine();
            ExtraUpdate();
        }
        else
        {
            animator.SetBool("isRunning", false);
            escaping = false;
            base.Update();
        }

        if(!canMove)
        {
            animator.SetBool("isRunning", false);
        }

        


    }
    protected override void Attack()
    {
        rb.velocity = Vector2.zero;
        if (!isNotAttacking) return;

        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        canMove = false;
        isNotAttacking = false;
        Vector2 flipDirection = (player.position - transform.position).normalized;
        if (flipDirection.x > 0 && isFacingLeft) Flip();
        else if (flipDirection.x < 0 && !isFacingLeft) Flip();
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.8f);
        GameObject newProjectile = Instantiate(olsProjectile, transform.position, Quaternion.identity);
        Ols_Projectile newOlsProjectile = newProjectile.GetComponent<Ols_Projectile>();
        newOlsProjectile.damage = attackDamage;
        Vector2 direction = (player.position - transform.position).normalized;
        newProjectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
        canMove = true;
        
        yield return new WaitForSeconds(attackCooldown);
        isNotAttacking = true;
    }

    protected override void Die()
    {
        rb.velocity = Vector2.zero;
        animator.Play("GetDamage");
        base.Die();

    }

    protected override void Chase()
    {
        animator.SetBool("isRunning", true);
        Vector2 direction;
        if (escaping)
        {
            // La dirección es INVERSA
            direction = (transform.position - player.position).normalized;
        }
        else
        {
            direction = (player.position - transform.position).normalized;
        }
        rb.velocity = direction * speed;

        if (direction.x > 0 && isFacingLeft) Flip();
        else if (direction.x < 0 && !isFacingLeft) Flip();
    }

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, escapeDistance);
        base.OnDrawGizmosSelected();


    }

}
