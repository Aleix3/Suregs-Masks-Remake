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


    // Índice de esta máscara en el MaskTreeManager
    private int MaskIndex => data != null
        ? System.Array.FindIndex(MaskTreeManager.Instance.masks, m => m == this)
        : -1;

    /// <summary>Obtiene el nivel de una rama directamente del árbol, sin depender de ActiveBranchIndex.</summary>
    private int GetBranchLevel(int branch)
    {
        int idx = MaskIndex;
        if (idx < 0 || MaskTreeManager.Instance == null) return 0;
        return MaskTreeManager.Instance.GetLevel(idx, branch);
    }

    // Índices de rama según el árbol visual (fila 1=0, fila 2=1, fila 3=2, fila 4=3)
    private const int BRANCH_DAMAGE = 0;   // fila 1 – daño
    private const int BRANCH_COOLDOWN = 1;   // fila 2 – cooldown
    private const int BRANCH_RAYS = 2;   // fila 3 – cantidad de rayos
    private const int BRANCH_POISON = 3;   // fila 4 – veneno

    protected override float GetEffectiveCooldown()
    {
        int cdLevel = GetBranchLevel(BRANCH_COOLDOWN);
        if (cdLevel > 0)
            return cooldownByLevel[cdLevel - 1];
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
        var target = GetStrongestEnemyInRoom();
        if (target == null) { Debug.Log("[Dinka] Sin objetivo."); return; }
        StartCoroutine(FireSequence(target));
    }

    [Header("Multi-rayo")]
    [Tooltip("Delay en segundos entre cada rayo")]
    public float delayBetweenRays = 0.3f;

    private System.Collections.IEnumerator FireSequence(Enemy firstTarget)
    {
        IsBusy = true;
        int dmgLevel = GetBranchLevel(BRANCH_DAMAGE);
        int rayLevel = GetBranchLevel(BRANCH_RAYS);
        int poisonLevel = GetBranchLevel(BRANCH_POISON);

        float damage = dmgLevel > 0 ? damageByLevel[dmgLevel - 1] : baseDamage;
        int count = rayLevel > 0 ? lightningCountByLevel[rayLevel - 1] : baseLightningCount;

        bool hasPoison = poisonLevel > 0;
        float poisonDmg = hasPoison ? poisonDamageByLevel[poisonLevel - 1] : 0f;
        float poisonDur = hasPoison ? poisonDurationByLevel[poisonLevel - 1] : 0f;

        // Construir lista de objetivos priorizando enemigos distintos
        var targets = BuildTargetList(firstTarget, count);

        for (int i = 0; i < targets.Count; i++)
        {
            Enemy t = targets[i];
            if (t == null || t.isDead) continue;

            t.TakeDamage((int)damage);
            if (hasPoison) t.ApplyPoison(poisonDmg, poisonDur, poisonTickRate);
            SpawnVFX(t.transform.position);

            if (i < targets.Count - 1)
                yield return new WaitForSeconds(delayBetweenRays);
        }

        IsBusy = false;
    }

    private System.Collections.Generic.List<Enemy> BuildTargetList(Enemy first, int count)
    {
        var list = new System.Collections.Generic.List<Enemy>();
        var inRoom = player.actualRoom?.enemiesInRoom;

        list.Add(first);

        if (count > 1 && inRoom != null)
        {
            // Ordenar el resto por vida descendente, excluyendo el primero
            var others = new System.Collections.Generic.List<Enemy>();
            foreach (Enemy e in inRoom)
            {
                if (e == null || e.isDead || e == first) continue;
                others.Add(e);
            }
            others.Sort((a, b) => b.health.CompareTo(a.health));

            int othersNeeded = count - 1;
            for (int i = 0; i < othersNeeded; i++)
            {
                if (i < others.Count)
                    list.Add(others[i]);        // enemigo distinto
                else
                    list.Add(first);            // fallback: repetir el primero
            }
        }

        return list;
    }


    private Enemy GetStrongestEnemyInRoom()
    {
        var enemies = player.actualRoom?.enemiesInRoom;
        if (enemies == null || enemies.Count == 0) return null;

        Enemy best = null;
        float maxHP = float.MinValue;

        foreach (Enemy e in enemies)
        {
            if (e == null || e.isDead) continue;
            if (e.health > maxHP) { maxHP = e.health; best = e; }
        }

        return best;
    }

    private void SpawnVFX(Vector3 pos)
    {
        if (lightningVFXPrefab) Instantiate(lightningVFXPrefab, pos, Quaternion.identity);
    }

}