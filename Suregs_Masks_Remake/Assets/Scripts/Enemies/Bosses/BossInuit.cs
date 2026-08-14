 using System.Collections;
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

    public GameObject wavePrefab;
    public GameObject boomerangPrefab;

    [Header("Attack Logic")]
    private int comboCounter = 0;
    private float attackTimer = 0f;
    private float waveTimer = 0f;

    public float comboInterval = 1f;
    public float waveInterval = 10f;

    [Header("Boomerang Offset")]
    public Vector2 boomerangOffset = new Vector2(0.5f, 0f);

    private bool isBusy = false;

    private bool isPhase2 = false;

    public AudioClip attack1Clip;
    public AudioClip attack2Clip;
    public AudioClip boomerangClip;
    public AudioClip dieClip;

    public Transform maskSpawnpoint;
    public GameObject MaskPrefab;

    [Header("Cached original values")]
    private float baseSpeed;
    private float baseComboInterval;
    private float baseWaveInterval;

    private void Awake()
    {
        baseSpeed = speed;
        baseComboInterval = comboInterval;
        baseWaveInterval = waveInterval;
    }

    protected override void Start()
    {
        base.Start();

        currentPhase = Phase.Phase1;
        life40 = maxHealth * 0.4f;

        player = Player.Instance.transform;
    }

    public override void ResetEnemy()
    {
        StopAllCoroutines();

        speed = baseSpeed;
        comboInterval = baseComboInterval;
        waveInterval = baseWaveInterval;

        isBusy = false;
        isPhase2 = false;
        comboCounter = 0;
        attackTimer = 0f;
        waveTimer = 0f;
        currentPhase = Phase.Phase1;

        bossState = BossState.Idle;
        desiredBossState = BossState.Idle;

        // Limpia el Animator (bools y triggers colgados)
        animator.SetBool("Phase2", false);
        animator.SetBool("IsMoving", false);
        animator.ResetTrigger("Attack1");
        animator.ResetTrigger("Attack2");
        animator.ResetTrigger("Wave");
        animator.ResetTrigger("PhaseChange");

        base.ResetEnemy();
    }

    protected override void Update()
    {
        if (player == null || isDead) return;

        attackTimer += Time.deltaTime;
        waveTimer += Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        if (health <= 0)
        {
            AudioManager.Instance.PlaySFX(dieClip);
            Instantiate(MaskPrefab, maskSpawnpoint.position, maskSpawnpoint.rotation);
            desiredBossState = BossState.Dead;
            BossStateMachine();
            return;
        }

        if (!isPhase2 && currentPhase == Phase.Phase1 && health <= life40)
        {
            desiredBossState = BossState.PhaseChange;
            BossStateMachine();
            isPhase2 = true;
            return;
        }


        if (currentPhase == Phase.Phase2)
        {
            comboInterval = 0.6f;
            waveInterval = 7f;
        }

        if (currentPhase == Phase.Phase2 && waveTimer >= waveInterval && !isBusy)
        {
            waveTimer = 0f;
            desiredBossState = BossState.WaveAttack;
            BossStateMachine();
            return;
        }

        if (attackTimer >= comboInterval && distance <= attackDistance && !isBusy)
        {
            attackTimer = 0f;
            desiredBossState = BossState.MeleeAttack;
            BossStateMachine();
            return;
        }

        desiredBossState = (distance <= viewDistance) ? BossState.Chase : BossState.Idle;

        BossStateMachine();
    }

    void BossStateMachine()
    {
        if (bossState == desiredBossState && desiredBossState != BossState.PhaseChange)
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
                break;

            case BossState.RangedAttack:
                RangedAttack();
                break;

            case BossState.WaveAttack:
                WaveAttack();
                break;

            case BossState.PhaseChange:
                ChangePhase();
                break;

            case BossState.Dead:
                Instantiate(MaskPrefab, maskSpawnpoint.position, maskSpawnpoint.rotation);
                Die();
                break;
        }

        bossState = desiredBossState;
    }

    void MeleeAttack()
    {
        if (isBusy) return;

        StartCoroutine(MeleeComboRoutine());
    }

    IEnumerator MeleeComboRoutine()
    {
        isBusy = true;
        agent.SetDestination(transform.position);
        rb.velocity = Vector2.zero;

        animator.SetTrigger("Attack1");
        AudioManager.Instance.PlaySFX(attack1Clip);
        yield return new WaitForSeconds(0.35f);

        animator.SetTrigger("Attack2");
        AudioManager.Instance.PlaySFX(attack2Clip);
        yield return new WaitForSeconds(0.35f);

        if (Vector2.Distance(transform.position, player.position) <= attackDistance)
        {
            player.GetComponent<Player>().TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(0.5f);

        comboCounter++;

        yield return StartCoroutine(BoomerangRoutine());

        if (comboCounter >= 3)
        {
            comboCounter = 0;
        }

        yield return new WaitForSeconds(comboInterval);

        isBusy = false;
    }

    IEnumerator BoomerangRoutine()
    {
        Vector3 spawnPos = transform.position + (Vector3)boomerangOffset;

        GameObject b = Instantiate(boomerangPrefab, spawnPos, Quaternion.identity);

        b.GetComponent<Boomerang>().animator.SetTrigger("Boomerang");
        AudioManager.Instance.PlaySFX(boomerangClip);
        Rigidbody2D rbB = b.GetComponent<Rigidbody2D>();

        Vector2 dir = (player.position - spawnPos).normalized;
        rbB.velocity = dir * 8f;

        yield return new WaitForSeconds(1f);

        while (b != null)
        {
            Vector2 returnDir = (transform.position - b.transform.position).normalized;
            rbB.velocity = returnDir * 10f;

            if (Vector2.Distance(b.transform.position, transform.position) < 0.5f)
                break;

            yield return null;
        }

        Destroy(b);

        if (comboCounter >= 3)
        {
            desiredBossState = BossState.WaveAttack;
        }
    }

    void WaveAttack()
    {
        if (isBusy) return;

        StartCoroutine(WaveRoutine());
    }

    IEnumerator WaveRoutine()
    {
        isBusy = true;

        animator.SetTrigger("Wave");

        Instantiate(wavePrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(1.5f);

        isBusy = false;
    }

    void ChangePhase()
    {
        base.StopCorroutinesGeneral();
        StartCoroutine(ChangePhaseRoutine());
    }

    IEnumerator ChangePhaseRoutine()
    {
        isBusy = true;

        rb.velocity = Vector2.zero;
        agent.SetDestination(transform.position);

        animator.SetTrigger("PhaseChange");

        yield return new WaitForSeconds(2f);

        currentPhase = Phase.Phase2;

        animator.SetBool("Phase2", true);

        speed *= 1.5f;
        attackDamage *= 1;

        attackTimer = 0f;
        waveTimer = 0f;

        isBusy = false;
        desiredBossState = BossState.Idle;
    }

    void RangedAttack()
    {
        StartCoroutine(BoomerangRoutine());
    }

    void UltimateAttack() { }

    protected override void Attack() { }
}