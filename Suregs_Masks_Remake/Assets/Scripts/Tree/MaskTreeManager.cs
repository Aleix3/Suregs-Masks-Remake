using UnityEngine;


public class MaskTreeManager : MonoBehaviour
{
    public static MaskTreeManager Instance { get; private set; }

    public const int MASK_COUNT = 4;
    public const int BRANCH_COUNT = 4;
    public const int MAX_LEVEL = 4;
    public const int MAX_UPGRADES = 8;

    private int[] _exp = new int[MASK_COUNT];

    public int[] expToLevel =
    {
        1000,
        1800,
        3000,
        3600,
        4500,
        6000,
        6600,
        8000
    };

    public int[] _points = new int[MASK_COUNT];

    public int[,] _levels = new int[MASK_COUNT, BRANCH_COUNT];

    [Header("Máscaras")]
    public BaseMask[] masks = new BaseMask[MASK_COUNT];

    public System.Action<int> OnTreeChanged;

    /// <summary>Se dispara cuando una máscara gana puntos. Parámetros: maskIndex, cantidad.</summary>
    public System.Action<int, int> OnPointsAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {

        LoadGame();
    }


    private const string KeyPoints = "MaskTree_Points_";
    private const string KeyLevel = "MaskTree_Level_";
    private const string KeyExp = "MaskTree_Exp_";

    public void SaveGame()
    {
        for (int m = 0; m < MASK_COUNT; m++)
        {
            PlayerPrefs.SetInt($"{KeyExp}{m}", _exp[m]);
            PlayerPrefs.SetInt($"{KeyPoints}{m}", _points[m]);

            for (int b = 0; b < BRANCH_COUNT; b++)
                PlayerPrefs.SetInt($"{KeyLevel}{m}_{b}", _levels[m, b]);
        }

        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        for (int m = 0; m < MASK_COUNT; m++)
        {
            _exp[m] = PlayerPrefs.GetInt($"{KeyExp}{m}", 0);
            _points[m] = PlayerPrefs.GetInt($"{KeyPoints}{m}", 0);

            for (int b = 0; b < BRANCH_COUNT; b++)
                _levels[m, b] = PlayerPrefs.GetInt($"{KeyLevel}{m}_{b}", 0);
        }

        RestoreAllMasks();

        for (int m = 0; m < MASK_COUNT; m++)
            OnTreeChanged?.Invoke(m);
    }

    public void AddExpPoints(int maskIndex, int amount)
    {
        if (!ValidMask(maskIndex))
            return;

        _exp[maskIndex] += amount;

        // Mientras haya suficiente XP para el siguiente punto
        while (_points[maskIndex] < MAX_UPGRADES &&
               _exp[maskIndex] >= expToLevel[_points[maskIndex]])
        {
            _points[maskIndex]++;
            OnPointsAdded?.Invoke(maskIndex, 1);
        }

        OnTreeChanged?.Invoke(maskIndex);
        SaveGame();
    }

    //public void AddExpPoints(int maskIndex, int amount)
    //{
    //    if (!ValidMask(maskIndex)) return;
    //    if(amount > expPointsToReachPerLevel)
    //    {
    //        _points[maskIndex] += amount;
    //    }


    //    OnTreeChanged?.Invoke(maskIndex);
    //    OnPointsAdded?.Invoke(maskIndex, amount);
    //    SaveGame();
    //}

    public int GetPoints(int maskIndex) =>
        ValidMask(maskIndex) ? _points[maskIndex] : 0;

    public bool TryDowngrade(int maskIndex, int branchIndex)
    {
        if (!ValidMask(maskIndex) || !ValidBranch(branchIndex)) return false;

        int currentLevel = _levels[maskIndex, branchIndex];
        if (currentLevel <= 0)
        {
            Debug.Log($"[SkillTree] Rama {branchIndex} de máscara {maskIndex} ya está en nivel 0.");
            return false;
        }

        // Devolver el punto gastado en ese nivel
        _points[maskIndex] += GetUpgradeCostForLevel(currentLevel - 1);
        _levels[maskIndex, branchIndex]--;

        // Actualizar la máscara
        DowngradeInMask(maskIndex, branchIndex);

        OnTreeChanged?.Invoke(maskIndex);
        SaveGame();
        return true;
    }

    private void DowngradeInMask(int maskIndex, int branchIndex)
    {
        BaseMask mask = masks[maskIndex];
        if (mask == null) return;

        // Reseleccionar la rama al nivel correcto desde 0
        mask.SelectBranch(branchIndex);
        int targetLevel = _levels[maskIndex, branchIndex];
        while (mask.ActiveBranchLevel < targetLevel)
            mask.UpgradeBranch();
    }

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

        _points[maskIndex] -= cost;
        _levels[maskIndex, branchIndex] += 1;

        ApplyToMask(maskIndex, branchIndex);

        OnTreeChanged?.Invoke(maskIndex);
        SaveGame();
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
        SaveGame();
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
        if (_levels[maskIndex, branchIndex] >= MAX_LEVEL) return false;
        if (GetTotalUpgrades(maskIndex) >= MAX_UPGRADES) return false;
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
                    bestLevel = _levels[m, b];
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
        public int[] points;        // [4]
        public int[] levels;        // [16], [4,4]
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
        level switch { 0 => 1, 1 => 1, 2 => 1, 3 => 1, _ => 99 };

    private static bool ValidMask(int i) => i >= 0 && i < MASK_COUNT;
    private static bool ValidBranch(int i) => i >= 0 && i < BRANCH_COUNT;
}