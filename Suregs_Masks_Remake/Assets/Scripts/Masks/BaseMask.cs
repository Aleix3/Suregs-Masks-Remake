using UnityEngine;


public abstract class BaseMask : MonoBehaviour
{

    [Header("Identidad")]
    public MaskData data;

    protected Player player;


    private float _cd;
    public float CurrentCooldown => _cd;
    public bool IsReady => _cd <= 0f;


    public int ActiveBranchIndex { get; private set; } = -1;
    public int ActiveBranchLevel { get; private set; } = 0;


    public float XP { get; private set; }
    public int MaskLevel { get; private set; }

    protected virtual void Awake() => player = GetComponentInParent<Player>();

    protected virtual void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;
    }


    public void TryActivate()
    {
        if (!IsReady) return;
        OnActivate();
        _cd = GetEffectiveCooldown();
    }

    public virtual void OnBasicAttack() { }
    public abstract void ApplyPassive();
    public abstract void RemovePassive();
    protected abstract void OnActivate();
    protected abstract float GetEffectiveCooldown();

    public void ReduceCooldown(float seconds) => _cd = Mathf.Max(0f, _cd - seconds);
    public void ForceReadyCooldown() => _cd = 0f;


    public void SelectBranch(int index)
    {
        if (index < 0 || index > 3) return;
        if (ActiveBranchIndex != index)
        {
            ActiveBranchIndex = index;
            ActiveBranchLevel = 0;
        }
    }

    public bool UpgradeBranch()
    {
        if (ActiveBranchIndex < 0 || ActiveBranchLevel >= 4) return false;
        ActiveBranchLevel++;
        return true;
    }

    public void ResetBranch() => ActiveBranchLevel = 0;


    public void AddXP(float amount)
    {
        XP += amount;
        float needed = 100f * (MaskLevel + 1);
        if (XP >= needed) { XP -= needed; MaskLevel++; OnLevelUp(); }
    }

    protected virtual void OnLevelUp() =>
        Debug.Log($"[{data?.maskName}] nivel de máscara {MaskLevel}");
}