using System.Collections;
using UnityEngine;

/// <summary>
/// Máscara #03 – Surma
///
/// STATS:   Vida -5% | Velocidad -5% | Daño -5% | Vel.Atq -5%
/// PASIVA:  Al bajar al 30% de vida gana +20% daño y +10% velocidad (mientras esté bajo)
/// ACTIVA:  Buff temporal (daño, velocidad, cadencia, vida)
///          Al terminar → debuff breve → luego entra en CD
///
/// Vida ejemplo: base 100 → con buff 150 → tras buff 80 (recupera gradual)
///
/// Árbol 0 – Estadísticas:     aumenta los bonos del buff
///           base +10% → niv1 15% / niv2 20% / niv3 25% / niv4 35%
/// Árbol 1 – Duración buff:    base 20s → 22s / 25s / 27s / 30s
/// Árbol 2 – Debilitamiento:   reduce duración y cantidad del debuff
///           base 10s -20% → niv1 8s / niv2 7s / niv3 5s / niv4 3s
/// Árbol 3 – Cooldown:         base 60s → 58s / 56s / 53s / 50s
/// </summary>
public class MaskSurma : BaseMask
{
    // ── Stats de máscara ──────────────────────────────────────────
    [Header("Stats aplicados al jugador")]
    public float statLife     = -0.05f;
    public float statSpeed    = -0.05f;
    public float statDamage   = -0.05f;
    public float statAtkSpeed = -0.05f;

    // ── Pasiva ────────────────────────────────────────────────────
    [Header("Pasiva – umbral de vida baja")]
    [Tooltip("Porcentaje de vida al que se activa (0.30 = 30%)")]
    public float passiveHealthThreshold  = 0.30f;
    public float passiveDamageBonus      = 0.20f;   // +20% daño
    public float passiveSpeedBonus       = 0.10f;   // +10% velocidad

    // ── Árbol 0: Stats del buff ───────────────────────────────────
    [Header("Árbol 0 – Stats del buff (multiplicador sobre stats base)")]
    public float baseBuff = 0.10f;                              // +10% a todo
    public float[] buffByLevel = { 0.15f, 0.20f, 0.25f, 0.35f };

    // ── Árbol 1: Duración del buff ────────────────────────────────
    [Header("Árbol 1 – Duración buff (segundos)")]
    public float baseDuration = 20f;
    public float[] durationByLevel = { 22f, 25f, 27f, 30f };

    // ── Árbol 2: Debilitamiento ───────────────────────────────────
    [Header("Árbol 2 – Duración del debuff (segundos)")]
    public float baseDebuffDuration = 10f;
    public float baseDebuffAmount   = 0.20f;   // -20% a stats durante el debuff
    public float[] debuffDurationByLevel = { 8f, 7f, 5f, 3f };
    // El debuff también se reduce en cantidad con el nivel (menos penalización)
    public float[] debuffAmountByLevel   = { 0.15f, 0.12f, 0.08f, 0.05f };

    // ── Árbol 3: Cooldown ─────────────────────────────────────────
    [Header("Árbol 3 – Cooldown (segundos)")]
    public float baseCooldown = 60f;
    public float[] cooldownByLevel = { 58f, 56f, 53f, 50f };

    // ── VFX ───────────────────────────────────────────────────────
    [Header("VFX")]
    public GameObject buffVFXPrefab;
    public GameObject debuffVFXPrefab;

    // ── Estado ────────────────────────────────────────────────────
    private bool      _passiveActive;
    private bool      _passiveTriggered;
    private bool      _buffActive;
    private Coroutine _buffRoutine;

    // ─────────────────────────────────────────────────────────────
    //  Cooldown
    // ─────────────────────────────────────────────────────────────
    protected override bool  ManualCooldown => true;

    protected override float GetEffectiveCooldown()
    {
        if (ActiveBranchIndex == 3 && ActiveBranchLevel > 0)
            return cooldownByLevel[ActiveBranchLevel - 1];
        return baseCooldown;
    }

    // ─────────────────────────────────────────────────────────────
    //  Pasiva — se evalúa en Update (vida baja)
    // ─────────────────────────────────────────────────────────────
    public override void ApplyPassive()  => _passiveActive = true;
    public override void RemovePassive()
    {
        _passiveActive = false;
        if (_passiveTriggered) RemovePassiveBuff();
    }

    protected override void Update()
    {
        base.Update();
        if (!_passiveActive) return;

        bool belowThreshold = (player.GetHealth() / player.GetMaxHealth()) <= passiveHealthThreshold;

        if (belowThreshold && !_passiveTriggered)       ApplyPassiveBuff();
        else if (!belowThreshold && _passiveTriggered)  RemovePassiveBuff();
    }

    private void ApplyPassiveBuff()
    {
        _passiveTriggered = true;
        player.BasicDamageMultiplier += passiveDamageBonus;
        player.SpeedMultiplier       += passiveSpeedBonus;
    }

    private void RemovePassiveBuff()
    {
        _passiveTriggered = false;
        player.BasicDamageMultiplier -= passiveDamageBonus;
        player.SpeedMultiplier       -= passiveSpeedBonus;
    }

    // ─────────────────────────────────────────────────────────────
    //  Activa — buff + debuff + CD en secuencia
    // ─────────────────────────────────────────────────────────────
    protected override void OnActivate()
    {
        if (_buffActive) return;   // no se puede reactivar mientras dura
        if (_buffRoutine != null) StopCoroutine(_buffRoutine);
        _buffRoutine = StartCoroutine(BuffSequence());
    }

    private IEnumerator BuffSequence()
    {
        // ── Parámetros según árbol ────────────────────────────────
        float buffAmount = ActiveBranchIndex == 0 && ActiveBranchLevel > 0
            ? buffByLevel[ActiveBranchLevel - 1]
            : baseBuff;

        float buffDuration = ActiveBranchIndex == 1 && ActiveBranchLevel > 0
            ? durationByLevel[ActiveBranchLevel - 1]
            : baseDuration;

        float debuffDuration = ActiveBranchIndex == 2 && ActiveBranchLevel > 0
            ? debuffDurationByLevel[ActiveBranchLevel - 1]
            : baseDebuffDuration;

        float debuffAmount = ActiveBranchIndex == 2 && ActiveBranchLevel > 0
            ? debuffAmountByLevel[ActiveBranchLevel - 1]
            : baseDebuffAmount;

        // ── FASE 1: BUFF ──────────────────────────────────────────
        _buffActive = true;

        player.BasicDamageMultiplier += buffAmount;
        player.SpeedMultiplier       += buffAmount;
        player.MaxHealthMultiplier   += buffAmount;
        // Subir la vida al nuevo máximo proporcionalmente
        player.HealToPercent(1f);

        if (buffVFXPrefab) Instantiate(buffVFXPrefab, player.transform.position, Quaternion.identity);

        Debug.Log($"[Surma] BUFF +{buffAmount * 100}% durante {buffDuration}s");

        yield return new WaitForSeconds(buffDuration);

        // ── FASE 2: QUITAR BUFF ───────────────────────────────────
        player.BasicDamageMultiplier -= buffAmount;
        player.SpeedMultiplier       -= buffAmount;
        player.MaxHealthMultiplier   -= buffAmount;
        _buffActive = false;

        // ── FASE 3: DEBUFF ────────────────────────────────────────
        player.BasicDamageMultiplier -= debuffAmount;
        player.SpeedMultiplier       -= debuffAmount;
        player.MaxHealthMultiplier   -= debuffAmount;
        // Reducir la vida al nuevo máximo si excede
        player.ClampHealthToMax();

        if (debuffVFXPrefab) Instantiate(debuffVFXPrefab, player.transform.position, Quaternion.identity);

        Debug.Log($"[Surma] DEBUFF -{debuffAmount * 100}% durante {debuffDuration}s");

        yield return new WaitForSeconds(debuffDuration);

        // ── FASE 4: QUITAR DEBUFF → ARRANCAR CD ──────────────────
        player.BasicDamageMultiplier += debuffAmount;
        player.SpeedMultiplier       += debuffAmount;
        player.MaxHealthMultiplier   += debuffAmount;

        Debug.Log("[Surma] Debuff terminado. CD iniciado.");

        // El CD arranca aquí (al terminar el debuff, no al pulsar la tecla)
        ForceStartCooldown(GetEffectiveCooldown());
    }
}
