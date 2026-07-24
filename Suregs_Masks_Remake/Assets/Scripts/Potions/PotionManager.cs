using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;

public class PotionManager : MonoBehaviour
{
    public static PotionManager instance { get; private set; }

    [Header("Input teclado")]
    public KeyCode prevPotionKey = KeyCode.R;
    public KeyCode nextPotionKey = KeyCode.T;
    public KeyCode consumeKey = KeyCode.E;

    [Header("Input mando (nombres configurados en Input Manager)")]
    public string gamepadPrevButton = "LB";
    public string gamepadNextButton = "RB";
    public string gamepadConsumeButton = "LT";

    [Header("Estado (solo lectura)")]
    public List<ItemType> potionTypes = new List<ItemType>();
    public int currentIndex = 0;


    public event System.Action<ItemType, Sprite, uint> OnPotionChanged;

    public event System.Action OnNoPotions;

    private InventoryManager inventoryManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (inventoryManager == null)
        {
            TryBindInventoryManager();
            return;
        }

        if (potionTypes.Count == 0) return;

        HandlePotionInput();
    }

    private void HandlePotionInput()
    {
        if (Input.GetKeyDown(prevPotionKey) || GetGamepadButtonDown(gamepadPrevButton))
            CyclePotion(-1);

        if (Input.GetKeyDown(nextPotionKey) || GetGamepadButtonDown(gamepadNextButton))
            CyclePotion(1);

        if (Input.GetKeyDown(consumeKey) || GetGamepadButtonDown(gamepadConsumeButton))
        {
            if (potionTypes.Count == 0) return;
            StartCoroutine(ConsumePotionRoutine());
        }
            
    }

    private bool GetGamepadButtonDown(string buttonName)
    {
        if (string.IsNullOrEmpty(buttonName)) return false;

        try { return Input.GetButtonDown(buttonName); }
        catch { return false; }
    }

    private void OnDestroy()
    {
        if (inventoryManager != null)
            inventoryManager.OnInventoryChanged -= RefreshPotionList;
    }

    private void TryBindInventoryManager()
    {
        if (InventoryManager.instance == null) return;

        inventoryManager = InventoryManager.instance;
        inventoryManager.OnInventoryChanged += RefreshPotionList;

        RefreshPotionList();
    }

    private bool IsPotion(ItemType type)
    {

        return type.ToString().StartsWith("POCION_") || type == ItemType.ORBE_MAGICO;
    }

    private void RefreshPotionList()
    {
        bool hadSelection = potionTypes.Count > 0;
        ItemType previousSelected = hadSelection && currentIndex < potionTypes.Count
            ? potionTypes[currentIndex]
            : default;

        potionTypes.Clear();

        foreach (InventoryItem item in inventoryManager.inventoryItems)
        {
            if (item == null) continue;
            if (item.quantity <= 0) continue;
            if (!IsPotion(item.type)) continue;

            potionTypes.Add(item.type);
        }

        if (potionTypes.Count == 0)
        {
            currentIndex = 0;
            OnNoPotions?.Invoke();
            return;
        }

        // intenta mantener seleccionada la misma pocion si sigue disponible
        if (hadSelection)
        {
            int keepIndex = potionTypes.IndexOf(previousSelected);
            currentIndex = keepIndex >= 0 ? keepIndex : 0;
        }
        else
        {
            currentIndex = 0;
        }

        NotifyCurrentPotion();
    }

    private void CyclePotion(int direction)
    {
        currentIndex += direction;

        if (currentIndex >= potionTypes.Count)
            currentIndex = 0;
        if (currentIndex < 0)
            currentIndex = potionTypes.Count - 1;

        NotifyCurrentPotion();
    }

    private void NotifyCurrentPotion()
    {
        if (potionTypes.Count == 0) return;

        ItemType current = potionTypes[currentIndex];
        uint quantity = (uint)inventoryManager.GetQuantity(current);

        GetItemData(current, out _, out _, out _, out Sprite sprite);

        OnPotionChanged?.Invoke(current, sprite, quantity);
    }

    public IEnumerator ConsumePotionRoutine()
    {
        
        Player.Instance.UsePotion();

        yield return new WaitForSeconds(0.833f);
        AudioManager.Instance.PlaySFX(AudioManager.Instance.usePotion);
        ItemType current = potionTypes[currentIndex];

        ApplyPotionEffect(current);
        inventoryManager.RemoveQuantity(current, 1);
    }

    // Valores segun la tabla de diseño de pociones
    private const float VIDA_1_PERCENT = 0.20f;
    private const float VIDA_2_PERCENT = 0.50f;
    private const float VIDA_3_PERCENT = 0.70f;

    private const float REGENERACION_PERCENT = 0.50f;
    private const float REGENERACION_DURATION = 5f;

    private const float DANO_BONUS_PERCENT = 0.20f;
    private const float DANO_DURATION = 10f;

    private const float VELOCIDAD_BONUS_PERCENT = 0.10f;
    private const float VELOCIDAD_DURATION = 20f;

    private void ApplyPotionEffect(ItemType type)
    {
        if (Player.Instance == null) return;

        switch (type)
        {
            case ItemType.POCION_VIDA_1:
                Player.Instance.HealPercentOfMax(VIDA_1_PERCENT);
                break;

            case ItemType.POCION_VIDA_2:
                Player.Instance.HealPercentOfMax(VIDA_2_PERCENT);
                break;

            case ItemType.POCION_VIDA_3:
                Player.Instance.HealPercentOfMax(VIDA_3_PERCENT);
                break;

            case ItemType.POCION_VIDA_MAX:
                Player.Instance.Heal(Player.Instance.MaxHealth);
                break;

            case ItemType.POCION_REGENERACION:
                Player.Instance.HealOverTime(REGENERACION_PERCENT, REGENERACION_DURATION);
                break;

            case ItemType.POCION_DANO:
                Player.Instance.ApplyTemporaryDamageBuff(DANO_BONUS_PERCENT, DANO_DURATION);
                break;

            case ItemType.POCION_VELOCIDAD:
                Player.Instance.ApplyTemporarySpeedBuff(VELOCIDAD_BONUS_PERCENT, VELOCIDAD_DURATION);
                break;

            case ItemType.ORBE_MAGICO:

                Player.Instance.MaskManager?.Primary?.ForceReadyCooldown();
                Player.Instance.MaskManager?.Secondary?.ForceReadyCooldown();
                break;
        }
    }


    public void PublishCurrentState()
    {
        if (potionTypes.Count > 0)
            NotifyCurrentPotion();
        else
            OnNoPotions?.Invoke();
    }

    public ItemType GetCurrentPotionType()
    {
        return potionTypes.Count > 0 ? potionTypes[currentIndex] : default;
    }
}