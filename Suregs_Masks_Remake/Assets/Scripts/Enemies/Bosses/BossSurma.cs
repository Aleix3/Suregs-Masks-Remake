using System.Collections;
using UnityEngine;


/// cinco golpes por combo. En fase dos, cada cuarto combo termina en
/// un golpe cargado que deja una explosión retardada.
/// Los impactos y los finales de animación se reciben mediante Animation Events.

public class BossSurma : Enemy
{
    private enum Phase { One, Changing, Two }
    private enum AttackType { Fast, ChargedTired, ChargedExplosion }

    [Header("Phase")]
    [SerializeField, Range(0.01f, 0.99f)] private float phaseTwoHealthPercent = 0.5f;
    [SerializeField] private float phaseTwoScaleMultiplier = 1.25f;
    [SerializeField] private float phaseOneSpeedMultiplier = 1.1f;
    [SerializeField] private float phaseTwoSpeedMultiplier = 1.3f;

    [Header("Combo")]
    [SerializeField, Min(1)] private int attacksPerCombo = 5;
    [SerializeField, Min(1)] private int normalCombosBeforeExplosion = 3;
    [SerializeField, Min(0f)] private float tiredDuration = 4f;
    [SerializeField, Min(0f)] private float explosionGroundDuration = 2f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private int explosionDamage = 20;
    [SerializeField, Range(0f, 1f)] private float playerSlowFactor = 0.55f;
    [SerializeField, Min(0f)] private float playerSlowDuration = 3f;
    [SerializeField] private GameObject slowVfxPrefab;
    [SerializeField] private float speedAfterExplosionHitMultiplier = 1.5f;

    [Header("Animator parameters")]
    [SerializeField] private string runningParameter = "isRunning";
    [SerializeField] private string fastAttackTrigger = "FastAttack";
    [SerializeField] private string chargedAttackTrigger = "ChargedAttack";
    [SerializeField] private string tiredTrigger = "Tired";
    [SerializeField] private string phaseChangeTrigger = "ChangePhase";
    [SerializeField] private string phaseChangeStateName = "Base Layer.changeFase";

    private Phase phase = Phase.One;
    private AttackType activeAttack;
    private int meleeHitsRemaining;
    private int fastHitsAllowedThisAnimation;
    private int completedPhaseTwoCombos;
    private bool isBusy;
    private bool waitingForExplosion;
    private bool explosionHitPlayer;
    private bool chargedDamageApplied;
    private bool currentComboIsExplosion;
    private Vector3 originalScale;
    private Coroutine playerSlowRoutine;
    private Player slowedPlayer;
    private float appliedSlowAmount;
    private GameObject slowVfxInstance;

    [Header("Explosion Prefab")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Transform explosionSpawnPoint;

    protected override void Start()
    {
        base.Start();
        originalScale = transform.localScale;
        RefreshMovementSpeed();
    }

    protected override void Update()
    {
        if (player == null || isDead)
            return;

        if (health <= 0)
        {
            Die();
            return;
        }

        if (phase == Phase.One && health <= maxHealth * phaseTwoHealthPercent)
        {
            BeginPhaseChange();
            return;
        }

        if (isBusy)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        FacePlayer();

        if (distance <= attackDistance)
            BeginNextAttack();
        else if (distance <= viewDistance)
            ChaseSurma();
        else
            SetIdle();
    }

    private void ChaseSurma()
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

    private void BeginNextAttack()
    {
        isBusy = true;
        animator.SetBool(runningParameter, false);
        agent.isStopped = true;
        agent.ResetPath();
        rb.velocity = Vector2.zero;
        FacePlayer();

        if (meleeHitsRemaining <= 0)
        {
            meleeHitsRemaining = attacksPerCombo;
            currentComboIsExplosion = phase == Phase.Two &&
                                      completedPhaseTwoCombos >= normalCombosBeforeExplosion;
        }

        bool isLastAttack = meleeHitsRemaining == 1;

        if (isLastAttack && currentComboIsExplosion)
            activeAttack = AttackType.ChargedExplosion;
        else if (isLastAttack && phase == Phase.One &&
                 Vector2.Distance(transform.position, player.position) <= attackDistance * 1.5)
            activeAttack = AttackType.ChargedTired;
        else
            activeAttack = AttackType.Fast;

        fastHitsAllowedThisAnimation = activeAttack == AttackType.Fast
            ? Mathf.Min(2, meleeHitsRemaining)
            : 0;
        chargedDamageApplied = false;
        animator.SetTrigger(activeAttack == AttackType.Fast ? fastAttackTrigger : chargedAttackTrigger);
    }

    // Evento en cada impacto de FastAttack y ChargedAttack.
    public void DealMeleeDamage()
    {
        if (!isBusy || player == null)
            return;

        bool canDealDamage = activeAttack == AttackType.Fast
            ? fastHitsAllowedThisAnimation-- > 0
            : !chargedDamageApplied;

        if (!canDealDamage)
            return;

        chargedDamageApplied = activeAttack != AttackType.Fast;
        meleeHitsRemaining--;
        if (Vector2.Distance(transform.position, player.position) <= attackDistance)
            player.GetComponent<Player>()?.TakeDamage(attackDamage);
    }

    // Evento una sola vez al final de FastAttack.
    public void FinishFastAttack()
    {
        if (!isBusy || activeAttack != AttackType.Fast)
            return;

        FinishCurrentStep();
    }

    // Evento al finalizar ChargedAttack cuando es el último golpe de fase uno.
    public void FinishChargedAttack()
    {
        if (!isBusy || activeAttack != AttackType.ChargedTired)
            return;

        StartCoroutine(TiredRoutine());
    }

    private IEnumerator TiredRoutine()
    {
        animator.SetTrigger(tiredTrigger);
        yield return new WaitForSeconds(tiredDuration);
        CompleteCombo();
    }

    // Evento al impactar el suelo durante el ChargedAttack especial de fase dos.
    public void BeginGroundedExplosion()
    {
        if (!isBusy || activeAttack != AttackType.ChargedExplosion || waitingForExplosion)
            return;

        waitingForExplosion = true;
        StartCoroutine(SpawnExplosionRoutine());
    }

    private IEnumerator SpawnExplosionRoutine()
    {

        if (explosionPrefab == null)
        {
            FinishExplosionAttack();
            yield break;
        }

        Vector3 spawnPosition = explosionSpawnPoint != null ? explosionSpawnPoint.position : transform.position;
        // El punto puede ser hijo del sprite con una Z distinta; el VFX debe
        // renderizarse en el mismo plano 2D que el boss, nunca detrás de cámara.
        spawnPosition.z = transform.position.z;
        GameObject explosion = Instantiate(explosionPrefab, spawnPosition, Quaternion.identity);
        SurmaExplosion explosionController = explosion.GetComponentInChildren<SurmaExplosion>();
        if (explosionController == null)
        {
            Debug.LogError("El prefab de explosión necesita SurmaExplosion.", explosion);
            FinishExplosionAttack();
            Destroy(explosion);
            yield break;
        }

        explosionController.Initialize(this);
    }

    // Evento en el frame dañino de la animación de explosión.
    public void DealExplosionDamage(Vector3 explosionPosition)
    {
        if (!waitingForExplosion || player == null)
            return;

        if (Vector2.Distance(explosionPosition, player.position) > explosionRadius)
            return;

        Player target = player.GetComponent<Player>();
        if (target == null)
            return;

        target.TakeDamage(explosionDamage);
        ApplySlowToPlayer(target);
        explosionHitPlayer = true;
    }

    private void ApplySlowToPlayer(Player target)
    {
        if (playerSlowRoutine != null)
            StopCoroutine(playerSlowRoutine);
        RestorePlayerSpeed();

        slowedPlayer = target;
        appliedSlowAmount = 1f - Mathf.Clamp01(playerSlowFactor);
        slowedPlayer.SpeedMultiplier -= appliedSlowAmount;

        if (slowVfxPrefab != null)
        {
            slowVfxInstance = Instantiate(
                slowVfxPrefab,
                slowedPlayer.transform.position,
                Quaternion.identity,
                slowedPlayer.transform);
        }

        playerSlowRoutine = StartCoroutine(RemovePlayerSlowAfterDelay());
    }

    private IEnumerator RemovePlayerSlowAfterDelay()
    {
        yield return new WaitForSeconds(playerSlowDuration);
        RestorePlayerSpeed();
        playerSlowRoutine = null;
    }

    private void RestorePlayerSpeed()
    {
        if (slowedPlayer != null && appliedSlowAmount > 0f)
            slowedPlayer.SpeedMultiplier += appliedSlowAmount;

        appliedSlowAmount = 0f;
        slowedPlayer = null;

        if (slowVfxInstance != null)
            Destroy(slowVfxInstance);
        slowVfxInstance = null;
    }

    // Evento al final de la animación de explosión.
    public void FinishExplosionAttack()
    {
        if (!isBusy || activeAttack != AttackType.ChargedExplosion)
            return;

        waitingForExplosion = false;
        if (explosionHitPlayer)
            RefreshMovementSpeed(speedAfterExplosionHitMultiplier);

        CompleteCombo();
    }

    private void FinishCurrentStep()
    {
        if (meleeHitsRemaining <= 0)
        {
            CompleteCombo();
            return;
        }

        isBusy = false;
        activeAttack = AttackType.Fast;
        agent.isStopped = false;
    }

    private void CompleteCombo()
    {
        meleeHitsRemaining = 0;
        if (phase == Phase.Two)
        {
            if (activeAttack == AttackType.ChargedExplosion)
                completedPhaseTwoCombos = 0;
            else
                completedPhaseTwoCombos++;
        }

        isBusy = false;
        activeAttack = AttackType.Fast;
        currentComboIsExplosion = false;
        explosionHitPlayer = false;
        agent.isStopped = false;
    }

    private void BeginPhaseChange()
    {
        phase = Phase.Changing;
        isBusy = true;
        StopAllCoroutines();
        agent.isStopped = true;
        agent.ResetPath();
        rb.velocity = Vector2.zero;
        animator.SetBool(runningParameter, false);

        // Un ataque disparado desde Any State puede tener un trigger pendiente.
        // Se limpian y se entra directamente en ChangeFase, garantizando prioridad.
        animator.ResetTrigger(fastAttackTrigger);
        animator.ResetTrigger(chargedAttackTrigger);
        animator.ResetTrigger(tiredTrigger);
        animator.ResetTrigger(phaseChangeTrigger);
        animator.Play(phaseChangeStateName, 0, 0f);
    }

    // Evento al final de ChangePhase.
    public void FinishPhaseChange()
    {
        if (phase != Phase.Changing)
            return;

        phase = Phase.Two;
        transform.localScale = transform.localScale * phaseTwoScaleMultiplier;
        RefreshMovementSpeed();
        isBusy = false;
        agent.isStopped = false;
    }

    private void RefreshMovementSpeed(float extraMultiplier = 1f)
    {
        float phaseMultiplier = phase == Phase.Two ? phaseTwoSpeedMultiplier : phaseOneSpeedMultiplier;
        agent.speed = initialSpeed * phaseMultiplier * extraMultiplier;
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
        Gizmos.color = Color.cyan;
        Vector3 explosionPosition = explosionSpawnPoint != null
            ? explosionSpawnPoint.position
            : transform.position;
        explosionPosition.z = transform.position.z;
        Gizmos.DrawWireSphere(explosionPosition, explosionRadius);
    }

    private void OnDisable()
    {
        RestorePlayerSpeed();
    }
}
