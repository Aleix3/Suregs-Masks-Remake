using System.Collections;
using UnityEngine;


public class EnemyKhurt : Enemy
{

    [Header("Khurt")]
    public float undergroundMoveDuration = 2f;
    public float undergroundSpeed = 4f;
    public float jumpAttackRange = 3f;
    public float jumpSpeed = 8f;
    public float stunDurationAfterJump = 3f;
    public Collider2D attackHitbox;

    public AudioClip attackClip;
    public AudioClip dieClip;

    private enum KhurtState
    {
        Idle, DiggingStart, DiggingMove, DiggingEmerge, Attacking, Stunned
    }

    private KhurtState _state = KhurtState.Idle;
    private bool _isPerformingAction = false;
    private bool _isUnderground = false;

    private SpriteRenderer _sr;

    protected override void Start()
    {
        base.Start();
        _sr = GetComponent<SpriteRenderer>();
        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    protected override void Update()
    {
        if (isDead) return;
        if (!roomConected.isPlayerInRoom) return;

        switch (_state)
        {
            case KhurtState.Idle: UpdateIdle(); break;
            case KhurtState.DiggingStart: break;
            case KhurtState.DiggingMove: break;
            case KhurtState.DiggingEmerge: break;
            case KhurtState.Attacking: break;
            case KhurtState.Stunned: break;
        }

        //animator.SetBool("isRunning", false);
    }
    private void UpdateIdle()
    {
        if (_isPerformingAction) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > viewDistance) return;

        //if (dist <= jumpAttackRange)
        //    StartCoroutine(JumpAttack());
        //else
            StartCoroutine(DigSequence());
    }

    private IEnumerator DigSequence()
    {
        _isPerformingAction = true;

        _state = KhurtState.DiggingStart;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;
        agent.ResetPath();

        animator.SetTrigger("DigIn");
        yield return new WaitForSeconds(0.6f);

        _isUnderground = true;

        _state = KhurtState.DiggingMove;
        animator.SetTrigger("Dig");

        float elapsed = 0f;

        while (elapsed < undergroundMoveDuration)
        {
            elapsed += Time.deltaTime;

            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * undergroundSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, player.position) <= jumpAttackRange)
                break;

            yield return null;
        }

        if (Vector2.Distance(transform.position, player.position) <= jumpAttackRange)
        {
            _isUnderground = false;

            yield return StartCoroutine(JumpAttack());
            yield break;
        }


        _state = KhurtState.DiggingEmerge;
        _isUnderground = false;

        animator.SetTrigger("DigOut");
        yield return new WaitForSeconds(0.5f);

        agent.updatePosition = true;
        agent.Warp(transform.position);
        agent.isStopped = false;

        _state = KhurtState.Idle;
        _isPerformingAction = false;
    }

    private IEnumerator JumpAttack()
    {
        _isPerformingAction = true;
        _state = KhurtState.Attacking;

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.updatePosition = false;
        agent.ResetPath();

        if (_isUnderground)
        {
            _isUnderground = false;

            
        }
        yield return new WaitForSeconds(0.3f);
        animator.SetTrigger("DigOut");
        yield return new WaitForSeconds(0.5f);

        _state = KhurtState.Stunned;
        animator.SetTrigger("Knock");

        yield return new WaitForSeconds(stunDurationAfterJump);

        animator.SetTrigger("BackToNormal");

        agent.updatePosition = true;
        agent.Warp(transform.position);
        agent.isStopped = false;

        _state = KhurtState.Idle;
        _isPerformingAction = false;
    }

    public override void TakeDamage(int damage)
    {
        if (_isUnderground) return;
        base.TakeDamage(damage);
    }
    protected override void Die()
    {
        StopAllCoroutines();
        agent.isStopped = true;
        //agent.updatePosition = true;
        if (attackHitbox != null) attackHitbox.enabled = false;
        AudioManager.Instance.PlaySFX(dieClip);
        animator.SetTrigger("Die");
        base.Die();
    }
    protected override void Attack() { }



    public void EnableAttackHitbox()
    {
        AudioManager.Instance.PlaySFX(attackClip);
        if (attackHitbox != null)
            attackHitbox.enabled = true;
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        Gizmos.color = new Color(0.6f, 0.4f, 0f);
        Gizmos.DrawWireSphere(transform.position, jumpAttackRange);
    }
}