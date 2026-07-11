using System.Collections;
using UnityEngine;


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

public class MaskSurma : BaseMask
{

    [Header("Stats aplicados al jugador")]
    public float statLife = -0.05f;
    public float statSpeed = -0.05f;
    public float statDamage = -0.05f;
    public float statAtkSpeed = -0.05f;


    [Header("Pasiva")]

    public float passiveHealthThreshold = 0.30f;
    public float passiveDamageBonus = 0.20f;
    public float passiveSpeedBonus = 0.10f;


    [Header("Árbol 0")]
    public float baseBuff = 0.10f;
    public float[] buffByLevel = { 0.15f, 0.20f, 0.25f, 0.35f };


    [Header("Árbol 1")]
    public float baseDuration = 20f;
    public float[] durationByLevel = { 22f, 25f, 27f, 30f };


    [Header("Árbol 2")]
    public float baseDebuffDuration = 10f;
    public float baseDebuffAmount = 0.20f;
    public float[] debuffDurationByLevel = { 8f, 7f, 5f, 3f };

    public float[] debuffAmountByLevel = { 0.15f, 0.12f, 0.08f, 0.05f };


    [Header("Árbol 3")]
    public float baseCooldown = 60f;
    public float[] cooldownByLevel = { 58f, 56f, 53f, 50f };


    [Header("VFX")]
    public GameObject buffVFXPrefab;
    public GameObject debuffVFXPrefab;


    private bool _passiveActive;
    private bool _passiveTriggered;
    private bool _buffActive;
    private Coroutine _buffRoutine;

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

    // Índices de rama según el árbol visual
    private const int BRANCH_STATS = 0;   // fila 1 – aumento de poder del buff
    private const int BRANCH_CDWN = 1;   // fila 2 – cooldown
    private const int BRANCH_DURATION = 2;   // fila 3 – tiempo de uso
    private const int BRANCH_DEBUFF = 3;   // fila 4 – robo de vida / debuff

    protected override float GetEffectiveCooldown()
    {
        int cdLevel = GetBranchLevel(BRANCH_CDWN);
        if (cdLevel > 0) return cooldownByLevel[cdLevel - 1];
        return baseCooldown;
    }


    public override void ApplyPassive() => _passiveActive = true;
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

        if (belowThreshold && !_passiveTriggered) ApplyPassiveBuff();
        else if (!belowThreshold && _passiveTriggered) RemovePassiveBuff();
    }

    private void ApplyPassiveBuff()
    {
        _passiveTriggered = true;
        player.BasicDamageMultiplier += passiveDamageBonus;
        player.SpeedMultiplier += passiveSpeedBonus;
    }

    private void RemovePassiveBuff()
    {
        _passiveTriggered = false;
        player.BasicDamageMultiplier -= passiveDamageBonus;
        player.SpeedMultiplier -= passiveSpeedBonus;
    }


    protected override void OnActivate()
    {
        if (_buffActive) return;
        if (_buffRoutine != null) StopCoroutine(_buffRoutine);
        _buffRoutine = StartCoroutine(BuffSequence());
    }

    private IEnumerator BuffSequence()
    {
        IsBusy = true;
        int buffLevel = GetBranchLevel(BRANCH_STATS);
        int cdwLevel = GetBranchLevel(BRANCH_CDWN);
        int debuffLevel = GetBranchLevel(BRANCH_DEBUFF);
        int durLevel = GetBranchLevel(BRANCH_DURATION);

        float buffAmount = buffLevel > 0 ? buffByLevel[buffLevel - 1] : baseBuff;
        float buffDuration = durLevel > 0 ? durationByLevel[durLevel - 1] : baseDuration;
        float debuffDuration = debuffLevel > 0 ? debuffDurationByLevel[debuffLevel - 1] : baseDebuffDuration;
        float debuffAmount = debuffLevel > 0 ? debuffAmountByLevel[debuffLevel - 1] : baseDebuffAmount;


        _buffActive = true;

        player.BasicDamageMultiplier += buffAmount;
        player.SpeedMultiplier += buffAmount;
        player.MaxHealthMultiplier += buffAmount;

        player.HealToPercent(1f);
        GameObject GO = null;
        if (buffVFXPrefab)
        {
            GO = Instantiate(buffVFXPrefab, player.transform.position, Quaternion.identity, player.transform);
        }

        Debug.Log($"[Surma] BUFF +{buffAmount * 100}% durante {buffDuration}s");

        yield return new WaitForSeconds(buffDuration);
        if (GO != null)
        {
            Destroy(GO);
        }

        //QUITAR BUFF
        player.BasicDamageMultiplier -= buffAmount;
        player.SpeedMultiplier -= buffAmount;
        player.MaxHealthMultiplier -= buffAmount;
        _buffActive = false;

        //DEBUFF
        player.BasicDamageMultiplier -= debuffAmount;
        player.SpeedMultiplier -= debuffAmount;
        player.MaxHealthMultiplier -= debuffAmount;

        player.ClampHealthToMax();

        GameObject GO2 = null;

        if (debuffVFXPrefab)
        {
            GO2 = Instantiate(debuffVFXPrefab, player.transform.position, Quaternion.identity, player.transform);
        }

        Debug.Log($"[Surma] DEBUFF -{debuffAmount * 100}% durante {debuffDuration}s");

        yield return new WaitForSeconds(debuffDuration);

        if (GO2 != null)
        {
            Destroy(GO2);
        }

        player.BasicDamageMultiplier += debuffAmount;
        player.SpeedMultiplier += debuffAmount;
        player.MaxHealthMultiplier += debuffAmount;

        Debug.Log("[Surma] Debuff terminado. CD iniciado.");


        ForceStartCooldown(GetEffectiveCooldown());
        IsBusy = false;
    }
}