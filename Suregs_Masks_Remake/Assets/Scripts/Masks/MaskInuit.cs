using System.Collections;
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


    protected override float GetEffectiveCooldown()
    {
        if (ActiveBranchIndex == 1 && ActiveBranchLevel > 0)
            return cooldownByLevel[ActiveBranchLevel - 1];
        return baseCooldown;
    }


    public override void ApplyPassive() => _passiveActive = true;
    public override void RemovePassive() => _passiveActive = false;

    public void TriggerPassiveWave()
    {
        if (!_passiveActive) return;

        float waveDamage = player.swordDamage * passiveWaveDamageFraction;
        var cols = Physics2D.OverlapCircleAll(player.transform.position, passiveWaveRadius, enemyLayer);
        foreach (var c in cols)
        {
            var e = c.GetComponent<Enemy>();
            if (e != null && !e.isDead) e.TakeDamage((int)waveDamage);
        }
        if (waveVFXPrefab) Instantiate(waveVFXPrefab, player.transform.position, Quaternion.identity);
    }

    // ─────────────────────────────────────────────────────────────
    //  Activa
    // ─────────────────────────────────────────────────────────────
    protected override void OnActivate() => StartCoroutine(BallRoutine());

    private IEnumerator BallRoutine()
    {
        //animación
        yield return new WaitForSeconds(0.4f);   // ajustar al timing del VFX

        float radius = baseExplosionRadius;
        if (ActiveBranchIndex == 2 && ActiveBranchLevel > 0)
            radius *= 1f + alcanceBonus * ActiveBranchLevel;  // +5% acumulativo

        float damage = ActiveBranchIndex == 0 && ActiveBranchLevel > 0
            ? damageByLevel[ActiveBranchLevel - 1]
            : baseDamage;

        bool hasPoison = ActiveBranchIndex == 3 && ActiveBranchLevel > 0;
        float poisonDmg = hasPoison ? poisonDamageByLevel[ActiveBranchLevel - 1] : 0f;
        float poisonDur = hasPoison ? poisonDurationByLevel[ActiveBranchLevel - 1] : 0f;

        var cols = Physics2D.OverlapCircleAll(transform.position, radius, enemyLayer);
        foreach (var c in cols)
        {
            var e = c.GetComponent<Enemy>();
            if (e == null || !!e.isDead) continue;
            e.TakeDamage((int)damage);
            if (hasPoison) e.ApplyPoison(poisonDmg, poisonDur, poisonTickRate);
        }

        if (explosionVFXPrefab)
        {
            var vfx = Instantiate(explosionVFXPrefab, transform.position, Quaternion.identity);
            vfx.transform.localScale = Vector3.one * (radius / baseExplosionRadius);
        }

    }

    private void OnDrawGizmosSelected()
    {
        float r = baseExplosionRadius;
        if (ActiveBranchIndex == 2 && ActiveBranchLevel > 0)
            r *= 1f + alcanceBonus * ActiveBranchLevel;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, r);
        Gizmos.color = new Color(0, 1, 1, 0.25f);
        Gizmos.DrawWireSphere(transform.position, passiveWaveRadius);
    }
}