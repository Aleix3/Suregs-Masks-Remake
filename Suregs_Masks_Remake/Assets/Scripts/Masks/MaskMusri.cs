using System.Collections;
using UnityEngine;

/// Máscara #02 – Musri
///
/// STATS:   Vida -20% | Velocidad +10% | Daño +10% | Vel.Atq +10%
/// PASIVA:  Cada dash del jugador lo vuelve invisible 2s
/// ACTIVA:  El jugador se vuelve invisible por un tiempo
///          Invisible = enemigos quietos + jugador invulnerable
///          Vuelve visible al atacar o al terminar el tiempo → empieza CD
///
/// Árbol 0 – Primer golpe invisible: bonus de daño al primer ataque mientras invisible
/// Árbol 1 – Cooldown:               base 20s → 18s / 16s / 13s / 10s
/// Árbol 2 – Distancia de dash:      aumenta el dashForce mientras la habilidad está activa
/// Árbol 3 – Duración invisibilidad: base 5s → 6s / 7.3s / 8.6s / 10s

public class MaskMusri : BaseMask
{
    [Header("Stats aplicados al jugador")]
    public float statLife = -0.20f;
    public float statSpeed = +0.10f;
    public float statDamage = +0.10f;
    public float statAtkSpeed = +0.10f;

    [Header("Pasiva")]
    public float passiveInvisDuration = 2f;

    // ── Árbol 0 – Primer golpe invisible ─────────────────────────
    [Header("Árbol 0 – Bonus daño primer golpe invisible")]
    [Tooltip("Multiplicador adicional de daño en el primer golpe (0.5 = +50%)")]
    public float[] firstHitBonusByLevel = { 0.25f, 0.50f, 0.75f, 1.00f };

    // ── Árbol 1 – Cooldown ────────────────────────────────────────
    [Header("Árbol 1 – Cooldown (segundos)")]
    public float baseCooldown = 20f;
    public float[] cooldownByLevel = { 18f, 16f, 13f, 10f };

    // ── Árbol 2 – Distancia de dash ───────────────────────────────
    [Header("Árbol 2 – Bonus distancia dash mientras invisible (+% del dashForce)")]
    public float[] dashBonusByLevel = { 0.25f, 0.50f, 0.75f, 1.00f };

    // ── Árbol 3 – Duración invisibilidad ─────────────────────────
    [Header("Árbol 3 – Duración invisibilidad (segundos)")]
    public float baseInvisDuration = 5f;
    public float[] invisDurationByLevel = { 6f, 7.3f, 8.6f, 10f };

    [Header("VFX")]
    public GameObject invisVFXPrefab;

    // ── Estado ────────────────────────────────────────────────────
    private bool _passiveActive;
    private bool _isInvisible;
    private bool _firstHitUsed;     // controla si ya se usó el bonus del primer golpe
    private bool _abilityInvisible; // true solo cuando la invisibilidad viene de la ACTIVA
    private Coroutine _invisRoutine;
    private bool _cdPending;

    // ── Helpers MaskTree ──────────────────────────────────────────
    private int MaskIndex => data != null
        ? System.Array.FindIndex(MaskTreeManager.Instance.masks, m => m == this)
        : -1;

    private int GetBranchLevel(int branch)
    {
        int idx = MaskIndex;
        if (idx < 0 || MaskTreeManager.Instance == null) return 0;
        return MaskTreeManager.Instance.GetLevel(idx, branch);
    }

    // ── Constantes de rama ────────────────────────────────────────
    private const int BRANCH_FIRST_HIT = 0;
    private const int BRANCH_COOLDOWN = 1;
    private const int BRANCH_DASH = 2;
    private const int BRANCH_DURATION = 3;

    // ── Cooldown ──────────────────────────────────────────────────
    protected override bool ManualCooldown => true;

    protected override float GetEffectiveCooldown()
    {
        int cdLevel = GetBranchLevel(BRANCH_COOLDOWN);
        if (cdLevel > 0) return cooldownByLevel[cdLevel - 1];
        return baseCooldown;
    }

    // ─────────────────────────────────────────────────────────────
    //  PASIVA — dash normal → invisibilidad breve
    // ─────────────────────────────────────────────────────────────
    public override void ApplyPassive() => _passiveActive = true;
    public override void RemovePassive() => _passiveActive = false;

    public void OnPlayerDash()
    {
        if (_passiveActive)
            StartInvis(passiveInvisDuration, fromAbility: false);
    }

    // ─────────────────────────────────────────────────────────────
    //  ACTIVA — invisibilidad principal
    // ─────────────────────────────────────────────────────────────
    protected override void OnActivate()
    {
        int durLevel = GetBranchLevel(BRANCH_DURATION);
        float invisDur = durLevel > 0 ? invisDurationByLevel[durLevel - 1] : baseInvisDuration;

        _cdPending = true;
        _firstHitUsed = false;
        StartInvis(invisDur, fromAbility: true);
    }

    // ─────────────────────────────────────────────────────────────
    //  ÁRBOL 2 — bonus de distancia durante invisibilidad activa
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// El Player llama a esto cada vez que hace un dash.
    /// Devuelve el multiplicador extra de dashForce (0 = sin bonus).
    /// Solo aplica si la invisibilidad viene de la habilidad activa.
    /// </summary>
    public float GetDashBonus()
    {
        if (!_abilityInvisible) return 0f;
        int dashLevel = GetBranchLevel(BRANCH_DASH);
        return dashLevel > 0 ? dashBonusByLevel[dashLevel - 1] : 0f;
    }

    // ─────────────────────────────────────────────────────────────
    //  ÁRBOL 0 — bonus de daño en el primer golpe invisible
    // ─────────────────────────────────────────────────────────────
    /// <summary>
    /// El Player llama a esto justo antes de aplicar daño de un autoataque.
    /// Devuelve el multiplicador extra (0 = sin bonus).
    /// Solo aplica una vez por activación de la habilidad.
    /// </summary>
    public float ConsumeFirstHitBonus()
    {
        if (!_isInvisible || _firstHitUsed) return 0f;
        int hitLevel = GetBranchLevel(BRANCH_FIRST_HIT);
        if (hitLevel <= 0) return 0f;

        _firstHitUsed = true;
        return firstHitBonusByLevel[hitLevel - 1];
    }

    // ─────────────────────────────────────────────────────────────
    //  Al atacar — romper invisibilidad
    // ─────────────────────────────────────────────────────────────
    public void OnPlayerAttacked()
    {
        if (!_isInvisible) return;
        if (_invisRoutine != null) StopCoroutine(_invisRoutine);
        ExitInvisibility();
    }

    // ─────────────────────────────────────────────────────────────
    //  Sistema de invisibilidad
    // ─────────────────────────────────────────────────────────────
    private void StartInvis(float duration, bool fromAbility)
    {
        if (_invisRoutine != null) StopCoroutine(_invisRoutine);
        _abilityInvisible = fromAbility;
        _invisRoutine = StartCoroutine(InvisRoutine(duration));
    }

    private IEnumerator InvisRoutine(float duration)
    {
        IsBusy = _abilityInvisible;
        _isInvisible = true;
        player.SetInvisible(true);
        if (invisVFXPrefab) Instantiate(invisVFXPrefab, player.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(duration);

        ExitInvisibility();
    }

    private void ExitInvisibility()
    {
        _isInvisible = false;
        _abilityInvisible = false;
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