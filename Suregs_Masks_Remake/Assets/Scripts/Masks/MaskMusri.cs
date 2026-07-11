using System.Collections;
using UnityEngine;


/// Máscara #02 – Musri
///
/// STATS:   Vida -20% | Velocidad +10% | Daño +10% | Vel.Atq +10%
/// PASIVA:  Cada dash del jugador lo vuelve invisible 2s
/// ACTIVA:  Dash que daña enemigos en el trayecto + invisibilidad 5s
///          Invisible = enemigos quietos + jugador invulnerable
///          Vuelve visible al atacar o al agotar el tiempo → entonces empieza el CD
///
/// Árbol 0 – Distancia:    base +1/3 dash → +5% acum. por nivel
/// Árbol 1 – Efectos:      enemigos tocados quedan aturdidos N segundos
/// Árbol 2 – Daño:         base 40 → 50 / 70 / 100 / 150
/// Árbol 3 – Invisibilidad: base 5s → 6s / 7.3s / 8.6s / 10s

public class MaskMusri : BaseMask
{

    [Header("Stats aplicados al jugador")]
    public float statLife = -0.20f;
    public float statSpeed = +0.10f;
    public float statDamage = +0.10f;
    public float statAtkSpeed = +0.10f;


    [Header("Pasiva")]
    public float passiveInvisDuration = 2f;


    [Header("Activa – valores base")]
    [Tooltip("Multiplicador del dashForce del jugador (0.33 = 1/3 más)")]
    public float dashExtraFactor = 0.33f;


    [Header("Árbol 0")]
    public float distanceBonusPerLevel = 0.05f;


    [Header("Árbol 1")]
    public float[] stunDurationByLevel = { 1f, 1.5f, 2f, 3f };
    public LayerMask enemyLayer;
    public float dashHitRadius = 1.5f;


    [Header("Árbol 2")]
    public float baseDamage = 40f;
    public float[] damageByLevel = { 50f, 70f, 100f, 150f };


    [Header("Árbol 3")]
    public float baseInvisDuration = 5f;
    public float[] invisDurationByLevel = { 6f, 7.3f, 8.6f, 10f };


    [Header("VFX")]
    public GameObject dashVFXPrefab;


    private bool _passiveActive;
    private bool _isInvisible;
    private Coroutine _invisRoutine;

    // Mientras es invisible el CD no corre al acabar
    private bool _cdPending;

    private int MaskIndex => data != null
        ? System.Array.FindIndex(MaskTreeManager.Instance.masks, m => m == this)
        : -1;

    private int GetBranchLevel(int branch)
    {
        int idx = MaskIndex;
        if (idx < 0 || MaskTreeManager.Instance == null) return 0;
        return MaskTreeManager.Instance.GetLevel(idx, branch);
    }


    protected override bool ManualCooldown => true;

    protected override float GetEffectiveCooldown()
    {
        int cdLevel = GetBranchLevel(1);
        if (cdLevel > 0) return cooldownByLevel[cdLevel - 1];
        return baseCooldown;
    }

    [Header("Árbol 1 – Cooldown (segundos)")]
    public float baseCooldown = 20f;
    public float[] cooldownByLevel = { 18f, 16f, 13f, 10f };


    public override void ApplyPassive() => _passiveActive = true;
    public override void RemovePassive() => _passiveActive = false;



    public void OnPlayerDash()
    {
        if (_passiveActive)
            StartInvis(passiveInvisDuration);
    }


    protected override void OnActivate()
    {
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        IsBusy = true;
        int distLevel = GetBranchLevel(0);
        int stunLevel = GetBranchLevel(1);
        int dmgLevel = GetBranchLevel(2);
        int invisLevel = GetBranchLevel(3);

        float damage = dmgLevel > 0 ? damageByLevel[dmgLevel - 1] : baseDamage;
        float distMult = 1f + dashExtraFactor + (distLevel > 0 ? distanceBonusPerLevel * distLevel : 0f);
        float stunDur = stunLevel > 0 ? stunDurationByLevel[stunLevel - 1] : 0f;
        float invisDur = invisLevel > 0 ? invisDurationByLevel[invisLevel - 1] : baseInvisDuration;


        Vector2 dir = player.lastMovementDirection == Vector2.zero
            ? Vector2.right
            : player.lastMovementDirection;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        rb.AddForce(dir * player.dashForce * distMult, ForceMode2D.Impulse);

        if (dashVFXPrefab) Instantiate(dashVFXPrefab, player.transform.position, Quaternion.identity);

        // Esperar un frame para que el dash se aplique y detectar impactos
        yield return new WaitForEndOfFrame();


        if (player.actualRoom != null)
        {
            foreach (Enemy e in player.actualRoom.enemiesInRoom)
            {
                if (e == null || e.isDead) continue;
                float dist = Vector2.Distance(player.transform.position, e.transform.position);
                if (dist <= dashHitRadius)
                {
                    e.TakeDamage((int)damage);
                    if (stunDur > 0f) e.ApplyStun(stunDur);
                }
            }
        }

        _cdPending = true;
        StartInvis(invisDur);
    }

    private void StartInvis(float duration)
    {
        if (_invisRoutine != null) StopCoroutine(_invisRoutine);
        _invisRoutine = StartCoroutine(InvisRoutine(duration));
    }

    private IEnumerator InvisRoutine(float duration)
    {
        _isInvisible = true;
        player.SetInvisible(true);

        yield return new WaitForSeconds(duration);

        ExitInvisibility();
    }


    public void OnPlayerAttacked()
    {
        if (!_isInvisible) return;
        if (_invisRoutine != null) StopCoroutine(_invisRoutine);
        ExitInvisibility();
    }

    private void ExitInvisibility()
    {

        _isInvisible = false;
        player.SetInvisible(false);
        IsBusy = false;

        if (_cdPending)
        {
            _cdPending = false;
            ForceStartCooldown(GetEffectiveCooldown());
        }
    }

    public bool IsInvisible => _isInvisible;
}