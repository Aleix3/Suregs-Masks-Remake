using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
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

    [SerializeField] public bool canMove = true;
    protected bool isNotAttacking = true;
    protected bool isFacingLeft = true;
    [SerializeField] public bool isStunned = false;

    public float attackCooldown;

    [SerializeField] public bool isDead = false;

    [SerializeField] public GameObject itemPrefab;

    private SpriteRenderer sr;

    private Coroutine flashRoutine;

    //PATHFINDING

    NavMeshAgent agent;

    protected virtual void Start()
    {
        health = maxHealth;
        currentState = EnemyState.Idle;
        desiredState = EnemyState.Idle;
        sr = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;


        for (int i = 0; i < 12; i++)
        {
            GameObject newItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);

            Item item = newItem.GetComponent<Item>();
            if (item != null)
            {
                if (i < 3)
                {
                    item.type = Item.ItemType.RUBI;
                }
                else if (3 < i && i < 5) 
                {
                    item.type = Item.ItemType.SALIVA;
                }
                else if (5 < i && i < 7)
                {
                    item.type = Item.ItemType.OJO;
                }
                else if (7 <= i && i < 8)
                {
                    item.type = Item.ItemType.DIENTE;
                }
                else if (8 < i && i < 10)
                {
                    item.type = Item.ItemType.COLA;
                }
                else if (10 <= i && i < 11)
                {
                    item.type = Item.ItemType.POLVORA;
                }
                else
                {
                    item.type = Item.ItemType.VISCERA;
                }
            }
        }
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
        else if (distance <= viewDistance && canMove)
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
        //rb.velocity = direction * speed;
        agent.SetDestination(player.position);

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
                newItem.AddComponent<ItemSpawnAnim>();
            }
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        if (currentState == EnemyState.Dead) return;

        health -= damage;
        print("health:" + health);

        // FLASH BLANCO AUTOMÁTICO
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashWhite(0.15f));

        if (health <= 0)
            desiredState = EnemyState.Dead;

    }

    protected void Flip()
    {
        isFacingLeft = !isFacingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator FlashWhite(float duration)
    {
        print("whiteee");
        Color original = sr.color;
        sr.color = Color.red;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(Color.red, original, t / duration);
            yield return null;
        }

        sr.color = original;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = viewColor;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Gizmos.color = attackColor;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        
    }
}
