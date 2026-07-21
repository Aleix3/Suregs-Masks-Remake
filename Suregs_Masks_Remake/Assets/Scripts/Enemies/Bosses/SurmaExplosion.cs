using UnityEngine;

/// <summary>Componente del prefab de explosión.</summary>
public class SurmaExplosion : MonoBehaviour
{
    private BossSurma owner;
    private bool initialized;

    public void Initialize(BossSurma boss)
    {
        owner = boss;
        initialized = true;
    }

    // Animation Event en el frame dañino del clip Idle del prefab.
    public void DealDamage()
    {
        if (initialized && owner != null)
            owner.DealExplosionDamage(transform.position);
    }

    // Animation Event al final del clip Idle del prefab.
    public void Finish()
    {
        if (initialized && owner != null)
            owner.FinishExplosionAttack();

        Destroy(this.transform.parent.gameObject);
    }
}
