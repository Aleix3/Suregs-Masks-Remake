using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum BossState
{
    Idle,
    Chase,
    MeleeAttack,
    RangedAttack,
    WaveAttack,
    Ultimate,
    PhaseChange,
    Dead
}

public class BossInuit : Enemy
{
    [Header("Boss")]
    public BossState bossState;
    private BossState desiredBossState;


    public enum Phase { Phase1, PhaseChange, Phase2 }
    public Phase currentPhase;

    private float life40;
    private float life5;

    public GameObject wavePrefab;

    [Header("Attack Logic")]
    private int comboCounter = 0;
    private float attackTimer = 0f;
    private float waveTimer = 0f;

    public float comboInterval = 1f;
    public float waveInterval = 10f;

    private bool alternateAttack = false;

    protected override void Start()
    {
        base.Start();

        currentPhase = Phase.Phase1;

        life40 = maxHealth * 0.4f;
        life5 = maxHealth * 0.05f;
    }

    protected override void Update()
    {
        if (player == null || isDead) return;

        attackTimer += Time.deltaTime;
        waveTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        // ---- FASES ----
        if (health <= life40 && currentPhase == Phase.Phase1)
        {
            currentPhase = Phase.PhaseChange;
            desiredBossState = BossState.PhaseChange;
            BossStateMachine();
            return;
        }

        // ---- MUERTE ----
        if (health <= 0)
        {
            desiredBossState = BossState.Dead;
            BossStateMachine();
            return;
        }

        // ---- ATAQUES ----

        // Wave cada X tiempo (fase 2)
        if (currentPhase == Phase.Phase2 && waveTimer >= waveInterval)
        {
            waveTimer = 0f;
            desiredBossState = BossState.WaveAttack;
            BossStateMachine();
            return;
        }

        

        // Melee cada 1s
        if (attackTimer >= comboInterval && distance <= attackDistance)
        {
            attackTimer = 0f;
            comboCounter++;
            desiredBossState = BossState.MeleeAttack;
            BossStateMachine();
            return;
        }

        // Movimiento
        if (distance <= viewDistance)
        {
            desiredBossState = BossState.Chase;
        }
        else
        {
            desiredBossState = BossState.Idle;
        }

        BossStateMachine();
    }

    void BossStateMachine()
    {
        if (bossState == desiredBossState)
            return;
        switch (desiredBossState)
        {
            case BossState.Idle:
                animator.SetBool("IsMoving", false);
                DoNothing();
                break;

            case BossState.Chase:
                animator.SetBool("IsMoving", true);
                Chase();
                break;

            case BossState.MeleeAttack:
                MeleeAttack();
                //print("melee");
                break;

            case BossState.RangedAttack:
                RangedAttack();
               
                break;

            case BossState.WaveAttack:
                WaveAttack();
                print("wavee");
                break;

            case BossState.Ultimate:
                UltimateAttack();
                break;

            case BossState.PhaseChange:
                ChangePhase();
                break;

            case BossState.Dead:
                Die();
                break;
            default:
                animator.SetBool("IsMoving", false);
                break;
        }

        bossState = desiredBossState;
    }

    void MeleeAttack()
    {
        agent.SetDestination(transform.position);

        if (isNotAttacking)
        {
            isNotAttacking = false;
            StartCoroutine(MeleeRoutine());
        }
    }

    IEnumerator MeleeRoutine()
    {
        rb.velocity = Vector2.zero;

        if (alternateAttack)
            animator.SetTrigger("Attack1");
        else
            animator.SetTrigger("Attack2");

        alternateAttack = !alternateAttack;

        yield return new WaitForSeconds(0.5f);

        if (Vector2.Distance(transform.position, player.position) <= attackDistance)
        {
            player.GetComponent<Player>().TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackCooldown);

        isNotAttacking = true;

        // Cada 3 combos boomerang
        if (comboCounter >= 3)
        {
            comboCounter = 0;
            desiredBossState = BossState.RangedAttack;
            BossStateMachine();
        }
    }

    public GameObject boomerangPrefab;

    void RangedAttack()
    {
        if (isNotAttacking)
        {
            isNotAttacking = false;

            StartCoroutine(BoomerangRoutine());
        }
    }

    IEnumerator BoomerangRoutine()
    {
        GameObject b = Instantiate(boomerangPrefab, transform.position, Quaternion.identity);

        b.GetComponent<Boomerang>().animator.SetTrigger("Boomerang");
        Vector2 dir = (player.position - transform.position).normalized;

        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        rb.velocity = dir * 8f;

        yield return new WaitForSeconds(1f);

        // volver
        while (b != null)
        {
            Vector2 returnDir = (transform.position - b.transform.position).normalized;
            rb.velocity = returnDir * 10f;

            if (Vector2.Distance(b.transform.position, transform.position) < 0.5f)
                break;

            yield return null;
        }

        Destroy(b);

        yield return new WaitForSeconds(attackCooldown);
        isNotAttacking = true;
    }



    void WaveAttack()
    {
        if (isNotAttacking)
        {
            isNotAttacking = false;

            animator.SetTrigger("Wave");

            StartCoroutine(WaveRoutine());
        }
    }

    IEnumerator WaveRoutine()
    {
        Instantiate(wavePrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(2f);

        isNotAttacking = true;
    }

    void ChangePhase()
    {
        rb.velocity = Vector2.zero;

        animator.SetTrigger("PhaseChange");

        StartCoroutine(ChangePhaseRoutine());
    }

    IEnumerator ChangePhaseRoutine()
    {
        yield return new WaitForSeconds(2f);

        currentPhase = Phase.Phase2;
        animator.SetBool("Phase2", true);

        speed *= 1.5f;
        attackDamage *= 1;

        desiredBossState = BossState.Idle;
    }

    void UltimateAttack()
    {
        if (isNotAttacking)
        {
            isNotAttacking = false;
            StartCoroutine(UltimateRoutine());
        }
    }

    IEnumerator UltimateRoutine()
    {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(wavePrefab, transform.position, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }

        isNotAttacking = true;
    }

    protected override void Attack()
    {
        // No se usa en el boss
    }

}
