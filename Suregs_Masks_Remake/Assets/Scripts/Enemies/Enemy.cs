using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Running, Attacking, Dead }

    [Header("Stats")]
    public int maxHealth;
    public int health;
    public float speed;
    public float attackDistance;
    public float viewDistance;
    public Color viewColor = Color.yellow;
    public int attackDamage;

    [Header("References")]
    public Rigidbody2D rb;
    //public Animator animator;
    public Transform player;

    protected EnemyState currentState;
    protected EnemyState desiredState;

    protected bool canAttack = true;
    protected bool isFacingLeft = true;
    private bool isStunned = false;

    protected float attackCooldown;

    protected virtual void Start()
    {
        health = maxHealth;
        currentState = EnemyState.Idle;
        desiredState = EnemyState.Idle;
    }

    protected virtual void ExtraUpdate() { }

    protected virtual void Update()
    {
        if (player == null || isStunned) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (health <= 0)
        {
            desiredState = EnemyState.Dead;
        }
        else if (distance <= attackDistance)
        {
            desiredState = EnemyState.Attacking;
        }
        else if (distance <= viewDistance)
        {
            desiredState = EnemyState.Running;
        }
        else
        {
            desiredState = EnemyState.Idle;
        }

        StateMachine();
        ExtraUpdate();
    }

    protected virtual void StateMachine()
    {
        switch (desiredState)
        {
            case EnemyState.Idle: DoNothing(); break;
            case EnemyState.Running: Chase(); break;
            case EnemyState.Attacking: Attack(); break;
            case EnemyState.Dead: Die(); break;
        }
        currentState = desiredState;
    }

    protected virtual void DoNothing()
    {
        rb.velocity = Vector2.zero;
        //animator.Play("Idle");
    }

    protected virtual void Chase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;

        if (direction.x > 0 && isFacingLeft) Flip();
        else if (direction.x < 0 && !isFacingLeft) Flip();

        //animator.Play("Run");
    }

    //Cada enemigo define su propio ataque
    protected abstract void Attack();

    protected virtual void Die()
    {
        rb.velocity = Vector2.zero;
        //animator.Play("Die");
        Destroy(gameObject, 1f);
    }

    public virtual void TakeDamage(int damage)
    {
        if (currentState == EnemyState.Dead) return;

        health -= damage;
        print("osiris health:" + health);
        //animator.Play("Hurt");

        if (health <= 0) desiredState = EnemyState.Dead;
    }

    protected void Flip()
    {
        isFacingLeft = !isFacingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = viewColor;
        Gizmos.DrawWireSphere(transform.position, viewDistance);
    }
}
