using UnityEngine;

public class MaskManager : MonoBehaviour
{
    public static MaskManager Instance { get; private set; }

    [Header("Máscaras iniciales")]
    public BaseMask slotPrimary;
    public BaseMask slotSecondary;

    [Header("Todas las máscaras del juego")]
    [SerializeField] private BaseMask[] allMasks;

    [Header("Input")]
    public KeyCode swapKey = KeyCode.R;
    public string swapButton = "Y";

    public BaseMask Primary { get; private set; }
    public BaseMask Secondary { get; private set; }

    public System.Action<BaseMask, BaseMask> OnSwap;
    public System.Action<BaseMask, BaseMask> OnSwapBlocked;
    public System.Action<BaseMask> OnActivateBlocked;

    private const string PrimaryMaskKey = "PrimaryMask";
    private const string SecondaryMaskKey = "SecondaryMask";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadGame();
    }

    private void Update()
    {
        HandleSwapInput();
        HandleActivateInput();

        Secondary?.ApplyPassive();
    }

    #region SAVE / LOAD

    public void SaveGame()
    {
        // Guardar máscaras equipadas
        if (Primary != null)
            PlayerPrefs.SetInt(PrimaryMaskKey, Primary.data.maskID);

        if (Secondary != null)
            PlayerPrefs.SetInt(SecondaryMaskKey, Secondary.data.maskID);

        // Guardar desbloqueos
        foreach (BaseMask mask in allMasks)
        {
            PlayerPrefs.SetInt(
                $"MaskUnlocked_{mask.data.maskID}",
                mask.data.isUnlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    private void LoadGame()
    {
        foreach (BaseMask mask in allMasks)
        {
            Debug.Log($"{mask.data.maskName} antes de cargar: {mask.data.isUnlocked}");
        }
        ResetUnlocksIfNoSave();

        foreach (BaseMask mask in allMasks)
        {
            mask.data.isUnlocked =
                PlayerPrefs.GetInt($"MaskUnlocked_{mask.data.maskID}", 0) == 1;
        }

        if (!PlayerPrefs.HasKey(PrimaryMaskKey))
        {
            SetPrimary(slotPrimary, slotSecondary);
            return;
        }

        int primaryId = PlayerPrefs.GetInt(PrimaryMaskKey);
        int secondaryId = PlayerPrefs.GetInt(SecondaryMaskKey);

        SetPrimary(GetMaskById(primaryId), GetMaskById(secondaryId));

        foreach (BaseMask mask in allMasks)
        {
            Debug.Log($"{mask.data.maskName} después de cargar: {mask.data.isUnlocked}");
        }
    }

    private void ResetUnlocksIfNoSave()
    {
        if (PlayerPrefs.HasKey(PrimaryMaskKey))
            return;

        foreach (BaseMask mask in allMasks)
        {
            mask.data.isUnlocked = false;
        }
    }

    private BaseMask GetMaskById(int id)
    {
        foreach (BaseMask mask in allMasks)
        {
            if (mask.data.maskID == id)
                return mask;
        }

        return null;
    }

    public void UnlockMask(MaskData data)
    {
        if (data == null)
            return;

        data.isUnlocked = true;

        SaveGame();
    }

    #endregion

    private void HandleSwapInput()
    {
        bool pressedKey = Input.GetKeyDown(swapKey);
        bool pressedButton = false;

        try { pressedButton = Input.GetButtonDown(swapButton); }
        catch { }

        if (pressedKey || pressedButton)
            Swap();
    }

    private void HandleActivateInput()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;
        if (Primary == null) return;

        if (!Primary.IsReady || Primary.IsLocked)
        {
            OnActivateBlocked?.Invoke(Primary);
            return;
        }

        Primary.TryActivate();
    }

    public bool IsSwapLocked =>
        (Primary != null && Primary.IsLocked) ||
        (Secondary != null && Secondary.IsLocked);

    public void Swap()
    {
        if (Primary == null || Secondary == null) return;

        if (IsSwapLocked)
        {
            Debug.Log("[MaskManager] Swap bloqueado: una máscara está en CD o en uso.");
            OnSwapBlocked?.Invoke(Primary, Secondary);
            return;
        }

        Secondary.RemovePassive();

        (Primary, Secondary) = (Secondary, Primary);

        OnSwap?.Invoke(Primary, Secondary);

        SaveGame();

        Debug.Log($"[MaskManager] Swap → Primaria: {Primary.data.maskName} | Secundaria: {Secondary.data.maskName}");
    }

    public void SetPrimary(BaseMask primary, BaseMask secondary)
    {
        Secondary?.RemovePassive();

        Primary = primary;
        Secondary = secondary;

        OnSwap?.Invoke(Primary, Secondary);

        SaveGame();
    }

    public void NotifyBasicAttack()
    {
        Primary?.OnBasicAttack();
    }

    public void GrantKillXP(float amount)
    {
        Primary?.AddXP(amount);
        Secondary?.AddXP(amount);
    }
}