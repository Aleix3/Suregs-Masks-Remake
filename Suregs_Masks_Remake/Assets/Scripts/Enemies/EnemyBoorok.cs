using System.Collections;
using UnityEngine;

public class EnemyBoorok : Enemy
{
    public float timeBeforeSleep = 10f;
    public float sleepHealPerSecond = 2f;
    public float wakeDetectionRadius = 3f;
    public Collider2D areaAttackHitbox;
    public float chargeTime = 0.75f;
    public float hitboxActiveDuration = 0.25f;
    public GameObject groundSlamVFXPrefab;

    public AudioClip attackClip;
    public AudioClip dieClip;

    private enum BoorokState
    {
        Sleeping, WakingUp, Moving, FallingAsleep, ChargingAttack, Attacking
    }

    private BoorokState _boorokState = BoorokState.Sleeping;
    private float _stateTimer = 0f;
    private bool _isPerformingAction = false;


    protected override void Start()
    {
        base.Start();
        if (areaAttackHitbox != null) areaAttackHitbox.enabled = false;
        EnterSleep(immediate: true);
    }


    protected override void Update()
    {
        if (isDead) return;
        if (isStunned) return;
        if (!roomConected.isPlayerInRoom) return;

        _stateTimer += Time.deltaTime;

        switch (_boorokState)
        {
            case BoorokState.Sleeping: UpdateSleeping(); break;
            case BoorokState.WakingUp: break;
            case BoorokState.Moving: UpdateMoving(); break;
            case BoorokState.FallingAsleep: break;
            case BoorokState.ChargingAttack: break;
            case BoorokState.Attacking: break;
        }

        bool isRunning = _boorokState == BoorokState.Moving &&
                         agent.velocity.magnitude > 0.05f &&
                         !agent.isStopped;
        animator.SetBool("isRunning", isRunning);
    }

    private void UpdateSleeping()
    {
        if (health < maxHealth)
            health = Mathf.Min(maxHealth,
                health + Mathf.RoundToInt(sleepHealPerSecond * Time.deltaTime));

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= wakeDetectionRadius)
            WakeUp();
    }

    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);
        if (_boorokState == BoorokState.Sleeping)
            WakeUp();
    }


    private void UpdateMoving()
    {
        if (_isPerformingAction) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackDistance)
        {
            StartCoroutine(ChargeAndAttack());
            return;
        }

        Chase();

        //if (_stateTimer >= timeBeforeSleep)
        //    StartCoroutine(FallAsleep());
    }

    private void WakeUp()
    {
        if (_boorokState == BoorokState.WakingUp ||
            _boorokState == BoorokState.Moving) return;

        StartCoroutine(WakeUpRoutine());
    }

    private IEnumerator WakeUpRoutine()
    {
        SetState(BoorokState.WakingUp);
        agent.isStopped = true;
        agent.ResetPath();

        animator.SetTrigger("WakeUp");
        yield return new WaitForSeconds(1.0f);

        SetState(BoorokState.Moving);
        agent.isStopped = false;
    }

    private void EnterSleep(bool immediate = false)
    {
        StartCoroutine(SleepRoutine(immediate));
    }

    private IEnumerator SleepRoutine(bool immediate)
    {
        SetState(BoorokState.FallingAsleep);
        agent.isStopped = true;
        agent.ResetPath();

        if (!immediate)
        {
            animator.SetTrigger("FallAsleep");
            yield return new WaitForSeconds(1f);
        }

        animator.SetTrigger("Sleep");
        SetState(BoorokState.Sleeping);
    }

    private IEnumerator FallAsleep()
    {
        _isPerformingAction = true;
        yield return StartCoroutine(SleepRoutine(immediate: false));
        _isPerformingAction = false;
    }

    protected override void Attack() 
    {


    }

    private IEnumerator ChargeAndAttack()
    {
        _isPerformingAction = true;
        SetState(BoorokState.ChargingAttack);

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;
        agent.ResetPath();

        animator.SetTrigger("ChargeAttack");
        AudioManager.Instance.PlaySFX(attackClip);
        yield return new WaitForSeconds(chargeTime + 1f);

        SetState(BoorokState.Attacking);
        animator.SetTrigger("Attack");

        if (areaAttackHitbox != null) areaAttackHitbox.enabled = true;
        if (groundSlamVFXPrefab != null)
        {
            Vector3 spawnPos = areaAttackHitbox != null
                ? areaAttackHitbox.bounds.center
                : transform.position;

            GameObject vfx = Instantiate(groundSlamVFXPrefab, spawnPos, Quaternion.identity);

            // Escalar el VFX para que coincida con el tamaño de la hitbox
            if (areaAttackHitbox != null)
            {
                Vector3 hitboxSize = areaAttackHitbox.bounds.size;
                vfx.transform.localScale = hitboxSize;
            }
        }

        yield return new WaitForSeconds(hitboxActiveDuration);
        if (areaAttackHitbox != null) areaAttackHitbox.enabled = false;

        yield return new WaitForSeconds(attackCooldown);

        agent.updatePosition = true;
        agent.isStopped = false;

        _stateTimer = 0f;
        SetState(BoorokState.Moving);
        _isPerformingAction = false;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        agent.ResetPath();
        AudioManager.Instance.PlaySFX(dieClip);
        if (areaAttackHitbox != null) areaAttackHitbox.enabled = false;
        animator.SetTrigger("Die");
        base.Die();
    }

    private void SetState(BoorokState newState)
    {
        _boorokState = newState;
        _stateTimer = 0f;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wakeDetectionRadius);
    }
}