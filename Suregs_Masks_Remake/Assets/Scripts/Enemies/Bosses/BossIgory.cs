using System.Collections;
using UnityEngine;


/// Boss de tres fases. Los impactos, el spawn de Suregs y el inicio del dash
/// se sincronizan desde Animation Events para coincidir con los sprites.

public class BossIgory : Enemy
{
    private enum Phase { One, Two, Three }
    private enum Action { None, BasicAttack, Summoning, Dashing, Healing }

    [Header("Phase thresholds")]
    [SerializeField, Range(0.01f, 0.99f)] private float phaseTwoHealthPercent = 0.8f;
    [SerializeField, Range(0.01f, 0.99f)] private float phaseThreeHealthPercent = 0.4f;
    [SerializeField] private float phaseOneSpeedMultiplier = 1f;
    [SerializeField] private float phaseTwoSpeedMultiplier = 1.2f;
    [SerializeField] private float phaseThreeSpeedMultiplier = 1.4f;

    [Header("Basic attacks")]
    [SerializeField, Min(0.01f)] private float phaseOneAttackInterval = 0.5f;
    [SerializeField, Min(0.01f)] private float phaseTwoAttackInterval = 0.4f;
    [SerializeField, Min(0.01f)] private float phaseThreeAttackInterval = 0.3f;

    [Header("Sureg summoning")]
    [SerializeField] private GameObject[] suregPrefabs;
    [SerializeField, Min(0.01f)] private float summonInterval = 10f;
    [SerializeField, Min(0f)] private float summonLockDuration = 5f;
    [SerializeField] private Transform[] suregSpawnPoints;
    [SerializeField] private float randomSpawnRadius = 1.5f;

    [Header("Dash (phases 2 and 3)")]
    [SerializeField] private float dashMinimumDistance = 7f;
    [SerializeField, Min(0.01f)] private float dashCooldown = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashHitRadius = 1.3f;
    [SerializeField] private int dashDamage = 30;
    [SerializeField] private float dashKnockbackForce = 14f;
    [SerializeField] private float dashKnockbackDuration = 0.3f;

    [Header("Phase 3 mask ability")]
    [SerializeField, Min(0.01f)] private float healAbilityCooldown = 20f;
    [SerializeField, Min(0.01f)] private float healAbilityDuration = 10f;
    [SerializeField, Min(0.01f)] private float healTickInterval = 2f;
    [SerializeField] private int healPerTick = 250;
    [SerializeField] private int shieldHealth = 1500;
    [SerializeField] private int healIfShieldSurvives = 1000;
    [SerializeField, Min(0f)] private float stunIfShieldBreaks = 2f;

    [Header("Aura VFX")]
    [SerializeField] private GameObject phaseTwoAuraPrefab;
    [SerializeField] private GameObject phaseThreeAuraPrefab;
    [SerializeField] private GameObject shieldAuraPrefab;

    [Header("Animator parameters")]
    [SerializeField] private string runningParameter = "isRunning";
    [SerializeField] private string attack1Trigger = "Attack1";
    [SerializeField] private string attack2Trigger = "Attack2";
    [SerializeField] private string attack3Trigger = "Attack3";
    [SerializeField] private string dashTrigger = "Dash";
    [SerializeField] private string healTrigger = "Heal";
    [SerializeField] private string summonTrigger = "GenerateSuregs";

    private Phase phase = Phase.One;
    private Action activeAction;
    private float attackTimer;
    private float summonTimer;
    private float dashTimer;
    private float healAbilityTimer;
    private bool dashDamageApplied;
    private bool shieldActive;
    private int currentShieldHealth;
    private Vector2 dashDirection;
    private GameObject phaseAuraInstance;
    private GameObject shieldAuraInstance;
    private Coroutine actionRoutine;
    private Coroutine healRoutine;

    protected override void Start()
    {
        base.Start();
        ApplyPhaseSpeed();
    }

    protected override void Update()
    {
        if (player == null || isDead || isStunned)
            return;

        if (health <= 0)
        {
            Die();
            return;
        }

        UpdatePhase();
        TickTimers();

        if (activeAction != Action.None)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        FacePlayer();

        if (phase == Phase.Three && healAbilityTimer <= 0f)
        {
            StartHealAbility();
            return;
        }

        if (summonTimer >= summonInterval)
        {
            StartSummoning();
            return;
        }

        if (phase != Phase.One && distance >= dashMinimumDistance && distance <= viewDistance && dashTimer <= 0f)
        {
            StartDash();
            return;
        }

        if (distance <= attackDistance && attackTimer <= 0f)
        {
            StartBasicAttack();
            return;
        }

        if (distance <= viewDistance)
            ChaseIgory();
        else
            SetIdle();
    }

    private void TickTimers()
    {
        attackTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        healAbilityTimer -= Time.deltaTime;
        summonTimer += Time.deltaTime;
    }

    private void UpdatePhase()
    {
        if (phase == Phase.One && health <= maxHealth * phaseTwoHealthPercent)
        {
            phase = Phase.Two;
            ApplyPhaseSpeed();
            ReplacePhaseAura(phaseTwoAuraPrefab);
        }

        if (phase == Phase.Two && health <= maxHealth * phaseThreeHealthPercent)
        {
            phase = Phase.Three;
            ApplyPhaseSpeed();
            ReplacePhaseAura(phaseThreeAuraPrefab);
            healAbilityTimer = 0f;
        }
    }

    private void ApplyPhaseSpeed()
    {
        float multiplier = phase == Phase.One ? phaseOneSpeedMultiplier :
                           phase == Phase.Two ? phaseTwoSpeedMultiplier : phaseThreeSpeedMultiplier;
        agent.speed = initialSpeed * multiplier;
    }

    private float CurrentAttackInterval()
    {
        return phase == Phase.One ? phaseOneAttackInterval :
               phase == Phase.Two ? phaseTwoAttackInterval : phaseThreeAttackInterval;
    }

    private void ChaseIgory()
    {
        animator.SetBool(runningParameter, true);
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void SetIdle()
    {
        animator.SetBool(runningParameter, false);
        DoNothing();
    }

    private void StartBasicAttack()
    {
        BeginAction(Action.BasicAttack);
        attackTimer = CurrentAttackInterval();

        int attack = Random.Range(0, 3);
        animator.SetTrigger(attack == 0 ? attack1Trigger : attack == 1 ? attack2Trigger : attack3Trigger);
    }

    private void StartSummoning()
    {
        BeginAction(Action.Summoning);
        summonTimer = 0f;
        animator.SetTrigger(summonTrigger);
        actionRoutine = StartCoroutine(EndActionAfter(summonLockDuration));
    }

    private void StartDash()
    {
        BeginAction(Action.Dashing);
        dashTimer = dashCooldown;
        dashDamageApplied = false;
        dashDirection = (player.position - transform.position).normalized;
        animator.SetTrigger(dashTrigger);
    }

    private void StartHealAbility()
    {
        BeginAction(Action.Healing);
        healAbilityTimer = healAbilityCooldown;
        animator.SetTrigger(healTrigger);
    }

    private void BeginAction(Action nextAction)
    {
        activeAction = nextAction;
        animator.SetBool(runningParameter, false);
        agent.isStopped = true;
        agent.ResetPath();
        rb.velocity = Vector2.zero;
    }

    private IEnumerator EndActionAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        EndAction();
    }

    // Animation Event en cada golpe visible de Attack1, Attack2 y Attack3.
    public void DealMeleeDamage()
    {
        if (activeAction != Action.BasicAttack || player == null)
            return;

        if (Vector2.Distance(transform.position, player.position) <= attackDistance)
            player.GetComponent<Player>()?.TakeDamage(attackDamage);
    }

    // Animation Event al final de Attack1, Attack2 y Attack3.
    public void FinishBasicAttack()
    {
        if (activeAction == Action.BasicAttack)
            EndAction();
    }

    // Animation Event en GenerateSuregs: genera exactamente dos prefabs aleatorios.
    public void SpawnSuregs()
    {
        if (activeAction != Action.Summoning || suregPrefabs == null || suregPrefabs.Length == 0)
            return;

        for (int i = 0; i < 2; i++)
        {
            GameObject prefab = suregPrefabs[Random.Range(0, suregPrefabs.Length)];
            if (prefab == null)
                continue;

            Vector3 position = GetSuregSpawnPosition(i);
            Instantiate(prefab, position, Quaternion.identity, roomConected.transform.Find("Enemies")?.transform);
            prefab.GetComponent<Enemy>().roomConected = roomConected;
            prefab.GetComponent<Enemy>().player = player;
        }
    }

    private Vector3 GetSuregSpawnPosition(int index)
    {
        if (suregSpawnPoints != null && index < suregSpawnPoints.Length && suregSpawnPoints[index] != null)
            return suregSpawnPoints[index].position;

        Vector2 offset = Random.insideUnitCircle * randomSpawnRadius;
        return transform.position + (Vector3)offset;
    }

    // Animation Event al comenzar el desplazamiento real del clip Dash.
    public void BeginDashMovement()
    {
        if (activeAction != Action.Dashing)
            return;

        rb.velocity = dashDirection * dashSpeed;
    }

    // Animation Event en el frame de impacto del Dash.
    public void DealDashDamage()
    {
        if (activeAction != Action.Dashing || dashDamageApplied || player == null)
            return;

        if (Vector2.Distance(transform.position, player.position) > dashHitRadius)
            return;

        Player target = player.GetComponent<Player>();
        if (target == null)
            return;

        dashDamageApplied = true;
        Vector2 knockbackDirection = (player.position - transform.position).normalized;
        target.TakeDamage(dashDamage);
        target.ApplyKnockback(knockbackDirection, dashKnockbackForce, dashKnockbackDuration);
    }

    // Animation Event al acabar Dash.
    public void FinishDash()
    {
        if (activeAction != Action.Dashing)
            return;

        rb.velocity = Vector2.zero;
        EndAction();
    }

    // Animation Event al comienzo de Heal: activa escudo y las curas periódicas.
    public void BeginMaskAbility()
    {
        if (activeAction != Action.Healing || shieldActive)
            return;

        shieldActive = true;
        currentShieldHealth = shieldHealth;
        if (shieldAuraPrefab != null)
            shieldAuraInstance = Instantiate(shieldAuraPrefab, transform.position, Quaternion.identity, transform);

        healRoutine = StartCoroutine(HealAbilityRoutine());
    }

    private IEnumerator HealAbilityRoutine()
    {
        float elapsed = 0f;
        while (elapsed < healAbilityDuration && shieldActive)
        {
            yield return new WaitForSeconds(healTickInterval);
            elapsed += healTickInterval;
            if (shieldActive)
                health = Mathf.Min(maxHealth, health + healPerTick);
        }

        if (shieldActive)
            EndMaskAbility(false);
    }

    /// <summary>El escudo absorbe el daño. Si se rompe, Igory queda aturdido.</summary>
    public override void TakeDamage(int damage)
    {
        if (shieldActive)
        {
            currentShieldHealth -= damage;
            if (currentShieldHealth <= 0)
                EndMaskAbility(true);
            return;
        }

        base.TakeDamage(damage);
    }

    private void EndMaskAbility(bool shieldBroken)
    {
        if (!shieldActive)
            return;

        shieldActive = false;
        currentShieldHealth = 0;
        if (healRoutine != null)
            StopCoroutine(healRoutine);
        healRoutine = null;

        if (shieldAuraInstance != null)
            Destroy(shieldAuraInstance);
        shieldAuraInstance = null;

        if (shieldBroken)
        {
            EndAction();
            ApplyStun(stunIfShieldBreaks);
        }
        else
        {
            health = Mathf.Min(maxHealth, health + healIfShieldSurvives);
            EndAction();
        }
    }

    // Animation Event al final de Heal. Solo cierra si el escudo no se rompió antes.
    public void FinishMaskAbility()
    {
        if (activeAction == Action.Healing && shieldActive)
            EndMaskAbility(false);
    }

    private void EndAction()
    {
        if (actionRoutine != null)
            StopCoroutine(actionRoutine);
        actionRoutine = null;
        rb.velocity = Vector2.zero;
        activeAction = Action.None;
        agent.isStopped = false;
    }

    private void ReplacePhaseAura(GameObject auraPrefab)
    {
        if (phaseAuraInstance != null)
            Destroy(phaseAuraInstance);
        if (auraPrefab != null)
            phaseAuraInstance = Instantiate(auraPrefab, transform.position, Quaternion.identity, transform);
    }

    private void FacePlayer()
    {
        if (player.position.x > transform.position.x && isFacingLeft)
            Flip();
        else if (player.position.x < transform.position.x && !isFacingLeft)
            Flip();
    }

    protected override void Attack() { }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dashMinimumDistance);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, dashHitRadius);
    }
}
