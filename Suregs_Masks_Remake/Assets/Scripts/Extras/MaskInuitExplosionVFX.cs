using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskExplosionVFX : MonoBehaviour
{

    [HideInInspector] public List<Enemy> targets = new List<Enemy>();
    [HideInInspector] public float damage;
    [HideInInspector] public bool hasPoison;
    [HideInInspector] public float poisonDmg;
    [HideInInspector] public float poisonDur;
    [HideInInspector] public float poisonTickRate;


    public void ApplyDamage()
    {
        foreach (Enemy e in targets)
        {
            if (e == null || e.isDead) continue;
            e.TakeDamage((int)damage);
            if (hasPoison) e.ApplyPoison(poisonDmg, poisonDur, poisonTickRate);
        }
    }

    private void Update()
    {
        this.transform.position = Player.Instance.transform.position;   
    }

    public void DestroyVFX()
    {
        Destroy(transform.parent.gameObject);
    }
}
