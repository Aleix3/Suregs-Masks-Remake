using System.Collections.Generic;
using UnityEngine;

public class MaskDinkaWaveVFX : MonoBehaviour
{
    [HideInInspector] public List<Enemy> targets = new();
    [HideInInspector] public float damage;

    public void ApplyDamage()
    {
        foreach (Enemy e in targets)
        {
            if (e == null || e.isDead)
                continue;

            e.TakeDamage(Mathf.RoundToInt(damage));
        }
    }

    public void DestroyVFX()
    {
        Destroy(gameObject);
    }
}