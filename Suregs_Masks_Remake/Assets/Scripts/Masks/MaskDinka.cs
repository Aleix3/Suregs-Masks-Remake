using System.Collections;
using UnityEngine;


/// Máscara #00 – Dinka
///
/// STATS:   Vida 0% | Velocidad -10% | Daño +10% | Vel.Atq +10%
/// PASIVA:  +20% daño en ataques básicos
/// ACTIVA:  Rayo al enemigo con más vida en rango
///
/// Árbol 0 – Daño:         base 50 → niv1 80 / niv2 130 / niv3 200 / niv4 300
/// Árbol 1 – Cooldown:     base 25s → 23s / 20s / 17s / 15s
/// Árbol 2 – Rayos:        base 1  → 2 / 3 / 4 / 5
/// Árbol 3 – Veneno:       base No → activa / dmg20 / dur9s / dmg30+dur11s

public class MaskDinka : BaseMask
{

    [Header("Stats aplicados al jugador")]
    public float statLife = 0.00f;
    public float statSpeed = -0.10f;
    public float statDamage = +0.10f;
    public float statAtkSpeed = +0.10f;


    [Header("Pasiva")]
    public float passiveBasicDamageBonus = 0.20f;


    [Header("Activa – valores base")]
    public float detectionRadius = 15f;
    public LayerMask enemyLayer;


    [Header("Árbol 0 – Daño")]
    public float baseDamage = 50f;
    public float[] damageByLevel = { 80f, 130f, 200f, 300f };


    [Header("Árbol 1 – Cooldown (segundos)")]
    public float baseCooldown = 25f;
    public float[] cooldownByLevel = { 23f, 20f, 17f, 15f };


    [Header("Árbol 2 – Cantidad de rayos")]
    public int baseLightningCount = 1;
    public int[] lightningCountByLevel = { 2, 3, 4, 5 };

    [Header("Árbol 3 – Veneno")]
    public float poisonTickRate = 1.5f;
    // nivel 0 = sin veneno
    // nivel 1 = activa veneno (daño 10, dur 6s)
    // nivel 2 = daño 20
    // nivel 3 = duración 9s
    // nivel 4 = daño 30, duración 11s
    public float[] poisonDamageByLevel = { 10f, 20f, 20f, 30f };
    public float[] poisonDurationByLevel = { 6f, 6f, 9f, 11f };


    [Header("VFX")]
    public GameObject lightningVFXPrefab;


    private bool _passiveApplied;


    protected override float GetEffectiveCooldown()
    {
        if (ActiveBranchIndex == 1 && ActiveBranchLevel > 0)
            return cooldownByLevel[ActiveBranchLevel - 1];
        return baseCooldown;
    }


    public override void ApplyPassive()
    {
        if (_passiveApplied) return;
        player.BasicDamageMultiplier += passiveBasicDamageBonus;
        _passiveApplied = true;
    }

    public override void RemovePassive()
    {
        if (!_passiveApplied) return;
        player.BasicDamageMultiplier -= passiveBasicDamageBonus;
        _passiveApplied = false;
    }

    protected override void OnActivate()
    {
        var target = GetStrongestEnemy();
        if (target == null) { Debug.Log("[Dinka] Sin objetivo."); return; }
        StartCoroutine(FireSequence(target));
    }

    private System.Collections.IEnumerator FireSequence(Enemy target)
    {
        float damage = ActiveBranchIndex == 0 && ActiveBranchLevel > 0
            ? damageByLevel[ActiveBranchLevel - 1]
            : baseDamage;

        int count = ActiveBranchIndex == 2 && ActiveBranchLevel > 0
            ? lightningCountByLevel[ActiveBranchLevel - 1]
            : baseLightningCount;

        bool hasPoison = ActiveBranchIndex == 3 && ActiveBranchLevel > 0;
        float poisonDmg = hasPoison ? poisonDamageByLevel[ActiveBranchLevel - 1] : 0f;
        float poisonDur = hasPoison ? poisonDurationByLevel[ActiveBranchLevel - 1] : 0f;

        for (int i = 0; i < count; i++)
        {
            if (target == null || target.isDead) break;

            target.TakeDamage((int)damage);
            if (hasPoison) target.ApplyPoison(poisonDmg, poisonDur, poisonTickRate);
            SpawnVFX(target.transform.position);

            if (i < count - 1)
                yield return new WaitForSeconds(0.15f);
        }
    }


    private Enemy GetStrongestEnemy()
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, detectionRadius, enemyLayer);
        Enemy best = null;
        float maxHP = float.MinValue;
        foreach (var c in cols)
        {
            var e = c.GetComponent<Enemy>();
            if (e == null || e.isDead) continue;
            if (e.health > maxHP) { maxHP = e.health; best = e; }
        }
        return best;
    }

    private void SpawnVFX(Vector3 pos)
    {
        if (lightningVFXPrefab) Instantiate(lightningVFXPrefab, pos, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}