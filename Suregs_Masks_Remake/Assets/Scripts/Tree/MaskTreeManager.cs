using UnityEngine;



/// Cada máscara tiene 4 ramas de 4 mejoras.
/// Máximo 8 mejoras activas por máscara.
/// 
/// Uso:
///   - Al matar un enemigo: MaskTreeManager.Instance.AddPoints(maskIndex, amount)
///   - Al comprar una mejora: MaskTreeManager.Instance.TryUpgrade(maskIndex, branchIndex)
///   - Para consultar estado: MaskTreeManager.Instance.GetState(maskIndex)

public class MaskTreeManager : MonoBehaviour
{
    public static MaskTreeManager Instance { get; private set; }

    public const int MASK_COUNT    = 4;
    public const int BRANCH_COUNT  = 4;
    public const int MAX_LEVEL     = 4;
    public const int MAX_UPGRADES  = 8;

    private int[] _points   = new int[MASK_COUNT];

    private int[,] _levels  = new int[MASK_COUNT, BRANCH_COUNT];

    [Header("Máscaras")]
    public BaseMask[] masks = new BaseMask[MASK_COUNT];

    public System.Action<int> OnTreeChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddPoints(int maskIndex, int amount)
    {
        if (!ValidMask(maskIndex)) return;
        _points[maskIndex] += amount;
        OnTreeChanged?.Invoke(maskIndex);
    }

    public int GetPoints(int maskIndex) =>
        ValidMask(maskIndex) ? _points[maskIndex] : 0;

    public bool TryUpgrade(int maskIndex, int branchIndex)
    {
        if (!ValidMask(maskIndex) || !ValidBranch(branchIndex)) return false;

        int currentLevel = _levels[maskIndex, branchIndex];

        if (currentLevel >= MAX_LEVEL)
        {
            Debug.Log($"[SkillTree] Rama {branchIndex} de máscara {maskIndex} ya está al máximo.");
            return false;
        }

        if (GetTotalUpgrades(maskIndex) >= MAX_UPGRADES)
        {
            Debug.Log($"[SkillTree] Máscara {maskIndex} ya tiene {MAX_UPGRADES} mejoras.");
            return false;
        }

        int cost = GetUpgradeCost(maskIndex, branchIndex);
        if (_points[maskIndex] < cost)
        {
            Debug.Log($"[SkillTree] Puntos insuficientes ({_points[maskIndex]}/{cost}).");
            return false;
        }

        _points[maskIndex]              -= cost;
        _levels[maskIndex, branchIndex] += 1;

        ApplyToMask(maskIndex, branchIndex);

        OnTreeChanged?.Invoke(maskIndex);
        return true;
    }

    public void ResetMask(int maskIndex)
    {
        if (!ValidMask(maskIndex)) return;

        for (int b = 0; b < BRANCH_COUNT; b++)
        {
            for (int lvl = 0; lvl < _levels[maskIndex, b]; lvl++)
                _points[maskIndex] += GetUpgradeCostForLevel(lvl);

            _levels[maskIndex, b] = 0;
        }

        if (masks[maskIndex] != null)
            masks[maskIndex].ResetBranch();

        OnTreeChanged?.Invoke(maskIndex);
    }

    public int GetLevel(int maskIndex, int branchIndex) =>
        ValidMask(maskIndex) && ValidBranch(branchIndex)
            ? _levels[maskIndex, branchIndex]
            : 0;

    public int GetTotalUpgrades(int maskIndex)
    {
        if (!ValidMask(maskIndex)) return 0;
        int total = 0;
        for (int b = 0; b < BRANCH_COUNT; b++)
            total += _levels[maskIndex, b];
        return total;
    }

    public bool CanUpgrade(int maskIndex, int branchIndex)
    {
        if (!ValidMask(maskIndex) || !ValidBranch(branchIndex)) return false;
        if (_levels[maskIndex, branchIndex] >= MAX_LEVEL)  return false;
        if (GetTotalUpgrades(maskIndex) >= MAX_UPGRADES)    return false;
        return _points[maskIndex] >= GetUpgradeCost(maskIndex, branchIndex);
    }

    public int GetUpgradeCost(int maskIndex, int branchIndex)
    {
        int nextLevel = _levels[maskIndex, branchIndex];
        return GetUpgradeCostForLevel(nextLevel);
    }

    private void ApplyToMask(int maskIndex, int branchIndex)
    {
        BaseMask mask = masks[maskIndex];
        if (mask == null) return;

        mask.SelectBranch(branchIndex);

        int targetLevel = _levels[maskIndex, branchIndex];
        while (mask.ActiveBranchLevel < targetLevel)
            mask.UpgradeBranch();
    }

    public void RestoreAllMasks()
    {
        for (int m = 0; m < MASK_COUNT; m++)
        {
            int bestBranch = 0, bestLevel = 0;
            for (int b = 0; b < BRANCH_COUNT; b++)
            {
                if (_levels[m, b] > bestLevel)
                {
                    bestLevel  = _levels[m, b];
                    bestBranch = b;
                }
            }
            if (bestLevel > 0)
                ApplyToMask(m, bestBranch);
        }
    }

    [System.Serializable]
    public struct SaveData
    {
        public int[]   points;        // [4]
        public int[]   levels;        // [16], [4,4]
    }

    public SaveData GetSaveData()
    {
        SaveData d = new SaveData();
        d.points = (int[])_points.Clone();
        d.levels = new int[MASK_COUNT * BRANCH_COUNT];
        for (int m = 0; m < MASK_COUNT; m++)
            for (int b = 0; b < BRANCH_COUNT; b++)
                d.levels[m * BRANCH_COUNT + b] = _levels[m, b];
        return d;
    }

    public void LoadSaveData(SaveData d)
    {
        _points = (int[])d.points.Clone();
        for (int m = 0; m < MASK_COUNT; m++)
            for (int b = 0; b < BRANCH_COUNT; b++)
                _levels[m, b] = d.levels[m * BRANCH_COUNT + b];
        RestoreAllMasks();
        for (int m = 0; m < MASK_COUNT; m++)
            OnTreeChanged?.Invoke(m);
    }


    private static int GetUpgradeCostForLevel(int level) =>
        level switch { 0 => 1, 1 => 1, 2 => 2, 3 => 2, _ => 99 };

    private static bool ValidMask(int i)   => i >= 0 && i < MASK_COUNT;
    private static bool ValidBranch(int i) => i >= 0 && i < BRANCH_COUNT;
}
