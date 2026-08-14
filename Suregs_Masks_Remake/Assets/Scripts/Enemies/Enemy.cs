using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


public abstract class Enemy : MonoBehaviour
{
    public enum EnemyState { Idle, Running, Attacking, Dead }
    public enum EnemyType { Osiris, Muur, Ols, Boorok, Khurt, OsirisVariation, MuurVariation, OlsVariation, BoorokVariation, KhurtVariation, BossInuit, BossMussri, BossSurma }

    [Header("Stats")]
    public EnemyType enemyType;
    public int maxHealth;
    public int health;
    public float speed;
    [SerializeField] public float initialSpeed;
    public float attackDistance;
    public float viewDistance;
    public Color viewColor = Color.yellow;
    public Color attackColor = Color.red;
    public int attackDamage;

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public Transform player;
    private MaskPointsOnKill maskPointsOnKill;

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
    private Color initialColor;

    private Coroutine flashRoutine;

    //PATHFINDING

    public NavMeshAgent agent;

    [SerializeField] public Room roomConected;

    protected virtual void Start()
    {
        initialSpeed = speed;
        health = maxHealth;
        currentState = EnemyState.Idle;
        desiredState = EnemyState.Idle;
        sr = GetComponent<SpriteRenderer>();
        initialColor = sr.color;
        player = Player.Instance.transform;

        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        animator = GetComponent<Animator>();
        maskPointsOnKill = GetComponent<MaskPointsOnKill>();

        //for (int i = 0; i < 12; i++)
        //{
        //    GameObject newItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);

        //    Item item = newItem.GetComponent<Item>();
        //    if (item != null)
        //    {
        //        if (i < 3)
        //        {
        //            item.type = Item.ItemType.RUBI;
        //        }
        //        else if (3 < i && i < 5) 
        //        {
        //            item.type = Item.ItemType.SALIVA;
        //        }
        //        else if (5 < i && i < 7)
        //        {
        //            item.type = Item.ItemType.OJO;
        //        }
        //        else if (7 <= i && i < 8)
        //        {
        //            item.type = Item.ItemType.DIENTE;
        //        }
        //        else if (8 < i && i < 10)
        //        {
        //            item.type = Item.ItemType.COLA;
        //        }
        //        else if (10 <= i && i < 11)
        //        {
        //            item.type = Item.ItemType.POLVORA;
        //        }
        //        else
        //        {
        //            item.type = Item.ItemType.VISCERA;
        //        }
        //    }
        //}
    }

    protected virtual void ExtraUpdate() { }

    protected virtual void Update()
    {

        if (player == null || isStunned || isDead || !roomConected.isPlayerInRoom) return;
        // BLOQUEAR IA mientras ataca
        if (!isNotAttacking || !canMove)
        {
            desiredState = EnemyState.Idle;

            agent.isStopped = true;
            agent.ResetPath();

            StateMachine();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        //if (health <= 0)
        //{
        //    desiredState = EnemyState.Dead;
        //}
        if (distance <= attackDistance)
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

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    protected virtual void Chase()
    {
        if (!canMove) return;
        if (!isNotAttacking) return;
        if (isDead) return;

        agent.isStopped = false;

        agent.SetDestination(player.position);

        Vector2 direction = (player.position - transform.position).normalized;

        if (direction.x > 0 && isFacingLeft) Flip();
        else if (direction.x < 0 && !isFacingLeft) Flip();
    }

    //Cada enemigo define su propio ataque
    protected abstract void Attack();

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        rb.velocity = Vector2.zero;
        roomConected.enemiesInRoom.Remove(this);
        maskPointsOnKill.GrantPoints();
        //animator.Play("Die");
        Destroy(gameObject, 1f);
    }

    private void OnDestroy()
    {
        if (!isDead) return;
        if (itemPrefab == null) return;

        float roll = Random.Range(0f, 100f);
        Item.ItemType? drop = null;

        switch (enemyType)
        {
            case EnemyType.Osiris:
                if (roll <= 60f)
                    drop = Item.ItemType.HUESO;
                break;

            case EnemyType.OsirisVariation:
                if (roll <= 60f)
                    drop = Item.ItemType.HUESO;
                break;

            case EnemyType.Muur:
                if (roll <= 30f)
                    drop = Item.ItemType.COLA;
                break;

            case EnemyType.MuurVariation:
                if (roll <= 45f)
                    drop = Item.ItemType.COLA;
                else if (roll <= 60f)
                    drop = Item.ItemType.POLVORA;
                break;

            case EnemyType.Ols:
                if (roll <= 30f)
                    drop = Item.ItemType.SALIVA;
                break;

            case EnemyType.OlsVariation:
                if (roll <= 40f)
                    drop = Item.ItemType.SALIVA;
                else if (roll <= 70f)
                    drop = Item.ItemType.OJO;
                break;

            case EnemyType.Khurt:
                if (roll <= 60f)
                    drop = Item.ItemType.GARRA;
                break;

            case EnemyType.KhurtVariation:
                if (roll <= 60f)
                    drop = Item.ItemType.GARRA;
                else if (roll <= 60f)
                    drop = Item.ItemType.DIENTE;
                else if (roll <= 80f)
                    drop = Item.ItemType.OJO;
                break;

            case EnemyType.Boorok:
                if (roll <= 35f)
                    drop = Item.ItemType.VISCERA;
                break;

            case EnemyType.BoorokVariation:
                if (roll <= 45f)
                    drop = Item.ItemType.VISCERA;
                else if (roll <= 75f)
                    drop = Item.ItemType.DIENTE;
                break;

            case EnemyType.BossInuit:
                    drop = Item.ItemType.ZAFIRO;
                break;

            case EnemyType.BossMussri:
                drop = Item.ItemType.RUBI;
                break;

            case EnemyType.BossSurma:
                drop = Item.ItemType.DIAMANTE;
                break;

        }

        if (!drop.HasValue)
            return;

        GameObject newItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);

        Item item = newItem.GetComponent<Item>();
        item.type = drop.Value;

        newItem.AddComponent<ItemSpawnAnim>();
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        if (currentState == EnemyState.Dead) return;

        health -= damage;
        print("health:" + health);

        // FLASH BLANCO
        if (flashRoutine == null)
            flashRoutine = StartCoroutine(FlashWhite(0.15f));
        else
        {
            StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashWhite(0.15f));
        }

        if (health <= 0)
        {
            Update();
            Die();
        }
            

    }

    public void Flip()
    {
        isFacingLeft = !isFacingLeft;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    IEnumerator FlashWhite(float duration)
    {

        Color original = initialColor;
        sr.color = Color.red;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            sr.color = Color.Lerp(Color.red, original, t / duration);
            yield return null;
        }

        sr.color = original;
        flashRoutine = null;
    }

    public void ApplySlow(float speedFactor, float duration)
    {
        StopCoroutine(nameof(SlowRoutine));
        StartCoroutine(SlowRoutine(speedFactor, duration));
    }

    private IEnumerator SlowRoutine(float factor, float duration)
    {
        speed = initialSpeed * factor;
        yield return new WaitForSeconds(duration);
        speed = initialSpeed;
    }

    public void ApplyPoison(float damagePerTick, float totalDuration, float tickRate)
    {
        StartCoroutine(DotRoutine(damagePerTick, totalDuration, tickRate));
    }

    public void ApplyBurn(float damagePerTick, float totalDuration, float tickRate)
    {
        StartCoroutine(DotRoutine(damagePerTick, totalDuration, tickRate));
    }

    private IEnumerator DotRoutine(float dmg, float duration, float rate)
    {
        float elapsed = 0f;
        while (elapsed < duration && !isDead)
        {
            yield return new WaitForSeconds(rate);
            elapsed += rate;
            TakeDamage((int)dmg);
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = viewColor;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Gizmos.color = attackColor;
        Gizmos.DrawWireSphere(transform.position, attackDistance);


    }

    public void StopCorroutinesGeneral()
    {
        sr.color = Color.white;
        StopAllCoroutines();
    }


    //  MÁSCARAS



    public void SetFrozen(bool frozen)
    {
        canMove = !frozen;

        if (agent != null)
        {
            agent.isStopped = frozen;
            if (frozen) agent.ResetPath();
        }

        rb.velocity = Vector2.zero;
    }

    public void ApplyStun(float duration)
    {
        if (isDead) return;
        StopCoroutine(nameof(StunRoutine));
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;
        SetFrozen(true);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        SetFrozen(false);
    }

    public virtual void ResetEnemy()
    {
        health = maxHealth;
        Start();
    }

}
