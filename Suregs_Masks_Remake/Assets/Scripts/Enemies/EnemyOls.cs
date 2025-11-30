using System.Collections;
using System.Collections.Generic;
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

        if (distance <= escapeDistance && isNotAttacking)
        {
            escaping = true;
            desiredState = EnemyState.Running;
            StateMachine();
            ExtraUpdate();
        }
        else
        {
            escaping = false;
            base.Update();
        }

        
    }
    protected override void Attack()
    {
        if (!isNotAttacking) return;

        rb.velocity = Vector2.zero;
        //animator.Play("Attack");

        StartCoroutine(DoAttack());
    }

    private IEnumerator DoAttack()
    {
        isNotAttacking = false;
        GameObject newProjectile = Instantiate(olsProjectile, transform.position, Quaternion.identity);
        Ols_Projectile newOlsProjectile = newProjectile.GetComponent<Ols_Projectile>();
        newOlsProjectile.damage = attackDamage;
        Vector2 direction = (player.position - transform.position).normalized;
        newProjectile.GetComponent<Rigidbody2D>().velocity = direction * projectileSpeed;
        yield return new WaitForSeconds(attackCooldown);
        isNotAttacking = true;
    }

    protected override void Die()
    {
        rb.velocity = Vector2.zero;
        //animator.Play("Die");
        base.Die();

    }

    protected override void Chase()
    {
        
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
