using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static Enemy;
using static Item;

public abstract class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Running, Attacking, Dead }
    public enum EnemyType { Osiris, Muur }

    [Header("Stats")]
    public EnemyType enemyType;
    public int maxHealth;
    public int health;
    public float speed;
    public float attackDistance;
    public float viewDistance;
    public Color viewColor = Color.yellow;
    public Color attackColor = Color.red;
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

    public float attackCooldown;

    private bool isDead = false;

    [SerializeField] public GameObject itemPrefab;

    protected virtual void Start()
    {
        health = maxHealth;
        currentState = EnemyState.Idle;
        desiredState = EnemyState.Idle;
    }

    protected virtual void ExtraUpdate() { }

    protected virtual void Update()
    {
        if (player == null || isStunned ||isDead) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (health <= 0)
        {
            desiredState = EnemyState.Dead;
        }
        else if (distance <= attackDistance)
        {
            desiredState = EnemyState.Attacking;
        }
        else if (distance <= viewDistance && canAttack)
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
        if(isDead) return;
        isDead = true;

        rb.velocity = Vector2.zero;
        //animator.Play("Die");
        Destroy(gameObject, 1f);
    }

    private void OnDestroy()
    {
        if (!isDead) return;

        if (itemPrefab != null)
        {
            GameObject newItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);

            Item item = newItem.GetComponent<Item>();
            if (item != null)
            {
                switch (enemyType)
                {
                    case EnemyType.Osiris:
                        item.type = Item.ItemType.HUESO;
                        break;
                    case EnemyType.Muur:
                        item.type = Item.ItemType.COLA;
                        break;
                }
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
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

        Gizmos.color = attackColor;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        
    }
}
