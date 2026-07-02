using System.Collections;
using UnityEngine;


public class EnemyBoorok : Enemy
{

    public float timeBeforeSleep     = 10f;
    public float sleepHealPerSecond  = 2f;
    public float wakeDetectionRadius = 3f;
    public Collider2D areaAttackHitbox;
    public float chargeTime          = 1.2f;
    public float hitboxActiveDuration = 0.25f;
    public GameObject groundSlamVFXPrefab;

    private enum BoorokState
    {
        Sleeping, WakingUp, Moving, FallingAsleep, ChargingAttack, Attacking
    }

    private BoorokState _boorokState = BoorokState.Sleeping;

    private float _stateTimer     = 0f;
    private bool  _isPerformingAction = false;

    protected override void Start()
    {
        base.Start();
        if (areaAttackHitbox != null) areaAttackHitbox.enabled = false;

        // Empieza dormido
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
            case BoorokState.Sleeping:      UpdateSleeping();      break;
            case BoorokState.WakingUp:      break;                  
            case BoorokState.Moving:        UpdateMoving();        break;
            case BoorokState.FallingAsleep: break;                  
            case BoorokState.ChargingAttack:break;                  
            case BoorokState.Attacking:     break;                  
        }

        bool isRunning = _boorokState == BoorokState.Moving &&
                         agent.velocity.magnitude > 0.1f;
        animator.SetBool("isRunning", isRunning);
    }


    private void UpdateSleeping()
    {
        if (health < maxHealth)
        {
            health = Mathf.Min(maxHealth,
                               health + Mathf.RoundToInt(sleepHealPerSecond * Time.deltaTime));
        }

        // Despertar si el jugador se acerca
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= wakeDetectionRadius)
            WakeUp();
    }


    public override void TakeDamage(int damage)
    {
        base.TakeDamage(damage);

        // Si recibe daño estando dormido, se despierta
        if (_boorokState == BoorokState.Sleeping)
            WakeUp();
    }

    private void UpdateMoving()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackDistance && !_isPerformingAction)
        {
            StartCoroutine(ChargeAndAttack());
            return;
        }

        if (!_isPerformingAction)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            Vector2 dir = (player.position - transform.position).normalized;
            if (dir.x > 0 && isFacingLeft)  Flip();
            else if (dir.x < 0 && !isFacingLeft) Flip();
        }

        // Dormirse tras N segundos en este estado
        if (_stateTimer >= timeBeforeSleep && !_isPerformingAction)
            StartCoroutine(FallAsleep());
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

        // Esperar a que termine la animación de despertar
        // (ajustar el tiempo a la duración real del clip)
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
        rb.velocity = Vector2.zero;

        if (!immediate)
        {
            animator.SetTrigger("FallAsleep");
            yield return new WaitForSeconds(0.8f);
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

    protected override void Attack() { }

    private IEnumerator ChargeAndAttack()
    {
        _isPerformingAction = true;

        SetState(BoorokState.ChargingAttack);

        agent.isStopped = true;
        agent.ResetPath();

        animator.SetTrigger("ChargeAttack");
        yield return new WaitForSeconds(chargeTime);

        SetState(BoorokState.Attacking);

        animator.SetTrigger("Attack");

        if (areaAttackHitbox != null) areaAttackHitbox.enabled = true;
        if (groundSlamVFXPrefab != null)
            Instantiate(groundSlamVFXPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(hitboxActiveDuration);

        if (areaAttackHitbox != null) areaAttackHitbox.enabled = false;

        yield return new WaitForSeconds(attackCooldown);

        _stateTimer = 0f;
        SetState(BoorokState.Moving);

        agent.isStopped = false;
        _isPerformingAction = false;
    }

    protected override void Die()
    {
        StopAllCoroutines();
        if (areaAttackHitbox != null) areaAttackHitbox.enabled = false;
        animator.SetTrigger("Die");
        base.Die();
    }

    private void SetState(BoorokState newState)
    {
        _boorokState = newState;
        _stateTimer  = 0f;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, wakeDetectionRadius);
    }
}
