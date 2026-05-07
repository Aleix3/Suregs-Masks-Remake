using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum MussriBossState
{
    Idle,
    Chase,
    KeepDistance,
    RangedAttack,
    MeleeAttack,
    Dash,
    ChargedArrow,
    InvisibleDash,
    PhaseTransition,
    Dead
}

public class BossMussri : Enemy
{
    [Header("Distances")]
    [SerializeField] private float rangedDistance = 8f;
    [SerializeField] private float meleeDistance = 2.5f;
    [SerializeField] private float keepDistance = 6f;

    [Header("Movement")]
    [SerializeField] private float dashForce = 12f;
    [SerializeField] private float invisibleDashForce = 16f;

    [Header("Cooldowns")]
    [SerializeField] private float rangedCooldown = 1.5f;
    [SerializeField] private float meleeCooldown = 2f;
    [SerializeField] private float chargedArrowCooldown = 7f;
    [SerializeField] private float invisibleDashCooldown = 8f;

    [Header("Projectiles")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject chargedArrowPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Phase 2")]
    [SerializeField] private float phase2HealthPercent = 0.4f;

    [Header("Components")]
    [SerializeField] private SpriteRenderer sr;
    // [SerializeField] private Animator animator;

    private MussriBossState currentBossState;

    private bool phase2 = false;
    private bool isInvisible = false;
    private bool isBusy = false;
    private bool transitioningPhase = false;

    private float rangedTimer;
    private float meleeTimer;
    private float chargedArrowTimer;
    private float invisibleDashTimer;

    private int attackCycle = 0;

    private bool currentDashInvisible = false;

    protected override void Start()
    {
        base.Start();

        currentBossState = MussriBossState.Idle;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        rangedTimer = 0f;
        meleeTimer = 0f;
        chargedArrowTimer = 0f;
        invisibleDashTimer = 0f;
    }

    protected override void Update()
    {
        if (player == null || isDead || isStunned)
            return;

        Cooldowns();

        CheckPhaseTransition();

        if (health <= 0)
        {
            currentBossState = MussriBossState.Dead;
            Die();
            return;
        }

        if (isBusy)
            return;

        float distance = Vector2.Distance(transform.position, player.position);

        HandleFlip();

        HandleCombatLogic(distance);
    }

    private void Cooldowns()
    {
        rangedTimer -= Time.deltaTime;
        meleeTimer -= Time.deltaTime;
        chargedArrowTimer -= Time.deltaTime;
        invisibleDashTimer -= Time.deltaTime;
    }

    private void CheckPhaseTransition()
    {
        if (phase2)
            return;

        if (health <= maxHealth * phase2HealthPercent)
        {
            StartCoroutine(PhaseTransition());
        }
    }

    private void HandleCombatLogic(float distance)
    {
        // MUY CERCA -> melee + dash
        if (distance <= meleeDistance)
        {
            if (meleeTimer <= 0)
            {
                StartCoroutine(MeleeAttack());
                return;
            }
        }

        // MUY LEJOS -> perseguir
        if (distance > rangedDistance)
        {
            currentBossState = MussriBossState.Chase;
            ChasePlayer();
            return;
        }

        // MUY CERCA PERO EN CD -> escapar
        if (distance < keepDistance * 0.7f)
        {
            currentBossState = MussriBossState.KeepDistance;
            KeepDistance();
            return;
        }

        // FASE 2 habilidades
        if (phase2)
        {
            if (chargedArrowTimer <= 0)
            {
                StartCoroutine(ChargedArrowAttack());
                return;
            }

            if (invisibleDashTimer <= 0)
            {
                StartCoroutine(SpecialDash());
                return;
            }
        }

        // ataque normal
        if (rangedTimer <= 0)
        {
            StartCoroutine(RangedAttack());
            return;
        }

        currentBossState = MussriBossState.Idle;
        animator.SetBool("isRunning", false);
        rb.velocity = Vector2.zero;
    }

    private void ChasePlayer()
    {
        if (!canMove)
            return;
        animator.SetBool("isRunning", true);
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void KeepDistance()
    {
        agent.isStopped = true;

        Vector2 dir = (transform.position - player.position).normalized;

        rb.velocity = dir * speed;
    }

    private void HandleFlip()
    {
        if (player == null)
            return;

        if (player.position.x > transform.position.x && isFacingLeft)
            Flip();
        else if (player.position.x < transform.position.x && !isFacingLeft)
            Flip();
    }

    protected override void Attack()
    {
        // NO USAR
    }

    private IEnumerator RangedAttack()
    {
        isBusy = true;

        currentBossState = MussriBossState.RangedAttack;

        rb.velocity = Vector2.zero;
        agent.isStopped = true;

        animator.SetTrigger("Attack");

        rangedTimer = rangedCooldown;

        yield return null;
    }

    private IEnumerator MeleeAttack()
    {
        isBusy = true;

        currentBossState = MussriBossState.MeleeAttack;

        canMove = false;

        rb.velocity = Vector2.zero;
        agent.isStopped = true;

        animator.SetTrigger("MeleeAttack");

        meleeTimer = meleeCooldown;

        yield return null;
    }

    private IEnumerator ChargedArrowAttack()
    {
        isBusy = true;

        currentBossState = MussriBossState.ChargedArrow;

        rb.velocity = Vector2.zero;
        agent.isStopped = true;

        animator.SetTrigger("ChargedAttack");

        chargedArrowTimer = chargedArrowCooldown;

        yield return null;
    }

    private IEnumerator PhaseTransition()
    {
        animator.SetBool("isPhaseChanging", true);

        phase2 = true;

        transitioningPhase = true;
        isBusy = true;

        currentBossState = MussriBossState.PhaseTransition;

        rb.velocity = Vector2.zero;

        agent.isStopped = true;

        canMove = false;

        yield return null;
    }

    protected override void Die()
    {
        if (isDead)
            return;

        currentBossState = MussriBossState.Dead;
        animator.SetBool("isDead", true);

        base.Die();
    }

    public void DoMeleeDamage()
    {
        Vector2 dir =
            (player.position - transform.position).normalized;

        Player ph =
            player.GetComponent<Player>();

        if (ph != null)
        {
            ph.TakeDamage(attackDamage);

            Rigidbody2D prb =
                player.GetComponent<Rigidbody2D>();

            if (prb != null)
            {
                prb.AddForce(
                    dir * 10f,
                    ForceMode2D.Impulse);
            }
        }

        StartCoroutine(DashBack(dir));
    }

    private IEnumerator SpecialDash()
    {
        isBusy = true;

        currentBossState = MussriBossState.Dash;

        animator.SetBool("isDashing", true);

        currentDashInvisible = true;

        isInvisible = true;

        sr.color = new Color(1, 1, 1, 0.2f);

        Vector2 dir =
            (transform.position - player.position).normalized;

        rb.velocity = dir * invisibleDashForce;

        invisibleDashTimer = invisibleDashCooldown;

        yield return null;
    }

    private IEnumerator DashBack(Vector2 dir)
    {
        currentBossState = MussriBossState.Dash;

        animator.SetBool("isDashing", true);

        currentDashInvisible = phase2;

        if (currentDashInvisible)
        {
            isInvisible = true;

            sr.color = new Color(1, 1, 1, 0.2f);
        }

        rb.velocity = -dir * dashForce;

        yield return null;
    }

    public void FireChargedArrow()
    {
        GameObject arrow =
            Instantiate(
                chargedArrowPrefab,
                shootPoint.position,
                Quaternion.identity);

        Vector2 dir =
            (player.position - shootPoint.position).normalized;

        arrow.GetComponent<ChargedArrowProjectile>()
            .SetDirection(dir);
    }

    public void FireNormalArrow()
    {
        GameObject arrow =
            Instantiate(
                arrowPrefab,
                shootPoint.position,
                Quaternion.identity);

        Vector2 dir =
            (player.position - shootPoint.position).normalized;

        arrow.GetComponent<ArrowProjectile>()
            .SetDirection(dir, 14f);
    }

    public void EndAttack()
    {
        isBusy = false;

        canMove = true;

        animator.SetBool("isDashing", false);

        rb.velocity = Vector2.zero;
    }

    public void FinishPhaseTransition()
    {
        speed += 1.5f;
        attackDamage += 10;

        canMove = true;

        transitioningPhase = false;
        isBusy = false;

        animator.SetBool("isPhaseChanging", false);
    }

    public void EndDash()
    {
        rb.velocity = Vector2.zero;

        animator.SetBool("isDashing", false);

        if (currentDashInvisible)
        {
            sr.color = Color.white;

            isInvisible = false;
        }

        currentDashInvisible = false;

        canMove = true;

        isBusy = false;
    }

    public void BecomeVisible()
    {
        sr.color = Color.white;

        isInvisible = false;
    }
}