using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// Máscara #01 – Inuit
///
/// STATS:   Vida -10% | Velocidad +10% | Daño +10% | Vel.Atq 0%
/// PASIVA:  Cada autoataque genera onda expansiva (daño = 1/3 del arma, alcance pequeño)
/// ACTIVA:  Bola de magia que explota en área alrededor del jugador al impactar el suelo
///
/// Árbol 0 – Daño:     base 100 → 120 / 180 / 250 / 400
/// Árbol 1 – Cooldown: base 30s → 28s / 25s / 23s / 20s
/// Árbol 2 – Alcance:  base Mediano → +5% / +5% / +5% / +5% (acumulativo)
/// Árbol 3 – Veneno:   base No → activa / dmg10 / dur10s / dmg15+dur15s

public class MaskInuit : BaseMask
{

    [Header("Stats aplicados al jugador")]
    public float statLife = -0.10f;
    public float statSpeed = +0.10f;
    public float statDamage = +0.10f;
    public float statAtkSpeed = 0.00f;


    [Header("Pasiva – onda expansiva")]
    [Tooltip("Fracción del daño del arma que hace la onda")]
    public float passiveWaveDamageFraction = 0.333f;
    public float passiveWaveRadius = 2.5f;
    public float passiveWaveBaseRadius = 2.5f;

    [Header("Activa – valores base")]
    public float baseExplosionRadius = 4f;
    public LayerMask enemyLayer;


    [Header("Árbol 0 – Daño")]
    public float baseDamage = 100f;
    public float[] damageByLevel = { 120f, 180f, 250f, 400f };


    [Header("Árbol 1 – Cooldown (segundos)")]
    public float baseCooldown = 30f;
    public float[] cooldownByLevel = { 28f, 25f, 23f, 20f };


    [Header("Árbol 2 – Alcance (bonus acumulativo por nivel, 0.05 = +5%)")]
    public float alcanceBonus = 0.05f;


    [Header("Árbol 3 – Veneno de zona")]
    // nivel 0 = sin veneno
    // nivel 1 = activa (zona veneno 5s, quita 5/s → base)
    // nivel 2 = daño 10
    // nivel 3 = duración 10s
    // nivel 4 = daño 15, duración 15s
    public float poisonTickRate = 1f;
    public float[] poisonDamageByLevel = { 5f, 10f, 10f, 15f };
    public float[] poisonDurationByLevel = { 5f, 5f, 10f, 15f };


    [Header("VFX")]
    public GameObject explosionVFXPrefab;
    public GameObject waveVFXPrefab;

    



    private bool _passiveActive;

    private int MaskIndex => data != null
        ? System.Array.FindIndex(MaskTreeManager.Instance.masks, m => m == this)
        : -1;

    private int GetBranchLevel(int branch)
    {
        int idx = MaskIndex;
        if (idx < 0 || MaskTreeManager.Instance == null) return 0;
        return MaskTreeManager.Instance.GetLevel(idx, branch);
    }


    protected override float GetEffectiveCooldown()
    {
        int cdLevel = GetBranchLevel(1);
        if (cdLevel > 0) return cooldownByLevel[cdLevel - 1];
        return baseCooldown;
    }


    public override void ApplyPassive() => _passiveActive = true;
    public override void RemovePassive() => _passiveActive = false;

    public void TriggerPassiveWave(Transform targetTransf)
    {
        if (!_passiveActive) return;

        float waveDamage = player.SwordDamage * passiveWaveDamageFraction;

        var targets = new List<Enemy>();

        Vector2 center = targetTransf.position;
        float radius = passiveWaveRadius;
        float radiusSqr = radius * radius;

        if (player.actualRoom != null)
        {
            foreach (Enemy e in player.actualRoom.enemiesInRoom)
            {
                if (e == null || e.isDead || e == targetTransf.GetComponent<Enemy>())
                    continue;

                if (((Vector2)e.transform.position - center).sqrMagnitude <= radiusSqr)
                    targets.Add(e);
            }
        }

        if (waveVFXPrefab)
        {
            var go = Instantiate(waveVFXPrefab, targetTransf.position, Quaternion.identity);

            // Escalar el efecto visual según el radio de la onda
            go.transform.localScale = Vector3.one * (radius / passiveWaveBaseRadius);

            var vfx = go.GetComponentInChildren<MaskDinkaWaveVFX>();

            if (vfx != null)
            {
                vfx.targets = targets;
                vfx.damage = waveDamage;
            }
        }
    }


    protected override void OnActivate()
    {
        int dmgLevel = GetBranchLevel(0);
        int rangeLevel = GetBranchLevel(2);
        int poisonLevel = GetBranchLevel(3);

        float radius = baseExplosionRadius;
        if (rangeLevel > 0) radius *= 1f + alcanceBonus * rangeLevel;

        float damage = dmgLevel > 0 ? damageByLevel[dmgLevel - 1] : baseDamage;

        bool hasPoison = poisonLevel > 0;
        float poisonDmg = hasPoison ? poisonDamageByLevel[poisonLevel - 1] : 0f;
        float poisonDur = hasPoison ? poisonDurationByLevel[poisonLevel - 1] : 0f;

        var targets = new System.Collections.Generic.List<Enemy>();

        if (player.actualRoom != null)
        {
            Vector2 center = player.transform.position;
            float radiusSqr = radius * radius;

            foreach (Enemy e in player.actualRoom.enemiesInRoom)
            {
                if (e == null || e.isDead)
                    continue;

                if (((Vector2)e.transform.position - center).sqrMagnitude <= radiusSqr)
                    targets.Add(e);
            }
        }

        if (explosionVFXPrefab)
        {
            var go = Instantiate(explosionVFXPrefab, player.transform.position, Quaternion.identity);
            go.transform.localScale = Vector3.one * (radius / baseExplosionRadius);

            var vfx = go.GetComponentInChildren<MaskExplosionVFX>();
            if (vfx != null)
            {
                vfx.targets = targets;
                vfx.damage = damage;
                vfx.hasPoison = hasPoison;
                vfx.poisonDmg = poisonDmg;
                vfx.poisonDur = poisonDur;
                vfx.poisonTickRate = poisonTickRate;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // GetBranchLevel requiere MaskTreeManager.Instance — solo disponible en runtime
        float r = baseExplosionRadius;
        if (MaskTreeManager.Instance != null)
        {
            int rangeLevel = GetBranchLevel(2);
            if (rangeLevel > 0) r *= 1f + alcanceBonus * rangeLevel;
        }
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, r);
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireSphere(transform.position, passiveWaveRadius);
    }
}