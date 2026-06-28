using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;



public class BlacksmithShop : MonoBehaviour
{
    public List<BlackSmithTrade> trades = new List<BlackSmithTrade>();
    public GameObject canvas;

    // Controla cuántos trades están pendientes de confirmar
    private Dictionary<int, int> pendingBuy = new Dictionary<int, int>();
    public List<BlackSmithTradeUI> tradeUIs;

    public Image swordImage;
    public Image armorImage;
    public event Action OnTradeUpdated;

    public ShopUI shopUI;

    public enum BlacksmithMode
    {
        Weapon,
        Armor
    }

    public BlacksmithMode currentMode;

    public void Interact()
    {
        canvas.SetActive(true);
        RefreshUI();
    }

    private void OnEnable()
    {
        UIState.IsUIOpen = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            canvas.SetActive(false);
            DialogueManager.Instance.CloseCommerce();
        }
            
    }
    public BlackSmithTrade GetCurrentTrade()
    {
        //Debug.Log($"Modo: {currentMode}");

        int targetLevel = currentMode == BlacksmithMode.Weapon
            ? Player.Instance.weaponLevel + 1
            : Player.Instance.armorLevel + 1;

        //Debug.Log($"Buscando nivel: {targetLevel}");

        //foreach (var t in trades)
        //{
        //    Debug.Log($"Trade: {t.potionResult} | Level: {GetLevel(t.potionResult)} | IsWeapon: {IsWeapon(t.potionResult)} | IsArmor: {IsArmor(t.potionResult)}");
        //}

        return trades.Find(t =>
        {
            bool correctType = currentMode == BlacksmithMode.Weapon
                ? IsWeapon(t.potionResult)
                : IsArmor(t.potionResult);

            return correctType && GetLevel(t.potionResult) == targetLevel;
        });
    }

    // 🔹 Lógica para seleccionar un trade
    public void SelectCurrentTrade()
    {
        BlackSmithTrade trade = GetCurrentTrade();

        if (trade == null)
        {
            Debug.Log("No hay mejora disponible");
            return;
        }

        int index = trades.IndexOf(trade);

        int alreadyPending = pendingBuy.ContainsKey(index) ? pendingBuy[index] : 0;

        int goldAvailable = PlayerEconomy.instance.GetGold() - (trade.goldCost * alreadyPending);
        if (goldAvailable < trade.goldCost)
        {
            for (int i = 0; i < tradeUIs.Count; i++)
            {
                if (tradeUIs[i].GetComponent<ShopButton>().isSelected)
                {
                    tradeUIs[i].GetComponent<ShopButton>().DeSelect(true);
                }
            }
            Debug.Log("No tienes suficiente oro");
            return;
        }

        if (trade.requiredItemQty > 0)
        {
            int qtyAvailable = InventoryManager.instance.GetQuantity(trade.requiredItem)
                             - (trade.requiredItemQty * alreadyPending);

            if (qtyAvailable < trade.requiredItemQty)
            {
                for (int i = 0; i < tradeUIs.Count; i++)
                {
                    if (tradeUIs[i].GetComponent<ShopButton>().isSelected)
                    {
                        tradeUIs[i].GetComponent<ShopButton>().DeSelect(true);
                    }
                }
                Debug.Log("No tienes suficientes materiales");
                return;
            }
        }

        for (int i = 0; i < tradeUIs.Count; i++)
        {
            if (tradeUIs[i].GetComponent<ShopButton>().isSelected)
            {
                tradeUIs[i].GetComponent<ShopButton>().SelectPermanent();
            }
        }

        if (!pendingBuy.ContainsKey(index))
            pendingBuy[index] = 0;

        pendingBuy[index]++;
    }

    public void CancelTrade()
    {
        for (int i = 0; i < shopUI.buttons.Count; i++)
        {
            if (shopUI.buttons[i].isSelected)
            {
                shopUI.buttons[i].DeSelect();
            }

        }
        pendingBuy.Clear();
        OnTradeUpdated?.Invoke();
    }

    // 🔹 Confirmar todas las compras
    public void ConfirmBuy()
    {
        foreach (var entry in pendingBuy)
        {
            BlackSmithTrade trade = trades[entry.Key];
            int qty = entry.Value;

            // Restar oro
            PlayerEconomy.instance.AddGold(-trade.goldCost * qty);

            // Restar items requeridos
            if (trade.requiredItemQty > 0)
            {
                InventoryManager.instance.RemoveQuantity(
                    trade.requiredItem,
                    (uint)(trade.requiredItemQty * qty)
                );
            }

            // Aplicar la mejora
            ApplyUpgrade(trade.potionResult, qty);
            //trades[entry.Key].ty = GetType()
            Debug.Log($"trade mejorado {trade.potionResult}");
        }
        for (int i = 0; i < tradeUIs.Count; i++)
        {
            if (tradeUIs[i].GetComponent<ShopButton>().isSelected)
            {
                tradeUIs[i].GetComponent<ShopButton>().DeSelect();
            }
        }
        RefreshUI();
        pendingBuy.Clear();
    }

    // 🔹 Aplica la mejora de arma o armadura
    private void ApplyUpgrade(ItemType type, int qty)
    {
        int level = GetLevel(type);
        if (level == 0)
        {
            Debug.LogWarning("Nivel inválido para: " + type);
            return;
        }

        if (IsWeapon(type))
        {
            Player.Instance.UpgradeSword(level);
        }
        else if (IsArmor(type))
        {
            Player.Instance.UpgradeArmor(level);
        }
        else
        {
            Debug.LogWarning("Tipo no reconocido para mejora: " + type);
        }
    }

    private int GetLevel(ItemType type)
    {
        string name = type.ToString();

        int lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore == -1)
            return 0;

        string levelPart = name.Substring(lastUnderscore + 1); // "NV2"

        levelPart = levelPart.Replace("NV", "");

        if (int.TryParse(levelPart, out int level))
            return level;

        return 0;
    }

    private string GetType(ItemType type)
    {
        string name = type.ToString();

        int lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore == -1)
            return name;

        return name.Substring(0, lastUnderscore);
    }

    private bool IsWeapon(ItemType type) => type.ToString().StartsWith("ESPADA");
    private bool IsArmor(ItemType type) => type.ToString().StartsWith("ARMADURA");

    // 🔹 Métodos auxiliares para ShopUI
    public int GetPending(ItemType type) => 0;
    public int GetRequiredItemQty(ItemType type)
    {
        int index = trades.FindIndex(t => t.potionResult == type);
        if (index == -1) return 0;
        return trades[index].requiredItemQty;
    }
    public int GetRequiredItemPending(ItemType type) => 0;
    public bool TryGetGoldValue(ItemType type, out int value)
    {
        int index = trades.FindIndex(t => t.potionResult == type);
        if (index == -1)
        {
            value = 0;
            return false;
        }

        value = trades[index].goldCost;
        return true;
    }

    public void SelectWeaponMode()
    {
        SetMode(BlacksmithMode.Weapon);
    }

    public void SelectArmorMode()
    {
        SetMode(BlacksmithMode.Armor);
    }

    public void SetMode(BlacksmithMode mode)
    {
        currentMode = mode;
    }

    ItemType GetWeaponTypeByLevel(int level)
    {
        return (ItemType)System.Enum.Parse(typeof(ItemType), $"ESPADA_NV{level}");
    }

    ItemType GetArmorTypeByLevel(int level)
    {
        return (ItemType)System.Enum.Parse(typeof(ItemType), $"ARMADURA_NV{level}");
    }

    public void RefreshUI()
    {
        int nextSwordLevel = Player.Instance.weaponLevel + 1;
        int nextArmorLevel = Player.Instance.armorLevel + 1;
        //obtener tipos
        ItemType swordType = GetWeaponTypeByLevel(nextSwordLevel);
        ItemType armorType = GetArmorTypeByLevel(nextArmorLevel);

        //obtener sprites desde el sistema
        Item.GetItemData(swordType, out _, out _, out _, out Sprite swordSprite);
        Item.GetItemData(armorType, out _, out _, out _, out Sprite armorSprite);

        swordImage.sprite = swordSprite;
        armorImage.sprite = armorSprite;

        for (int i = 0; i < tradeUIs.Count; i++)
        {
            tradeUIs[i].Refresh();
        }

        for (int i = 0; i < tradeUIs.Count; i++)
        {
            if(tradeUIs[i].mode == BlacksmithMode.Weapon)
            {
                tradeUIs[i].GetComponent<ShopButton>().itemType = swordType;
            }
            else
            {
                tradeUIs[i].GetComponent<ShopButton>().itemType = armorType;
            }
            
        }


    }

    public BlackSmithTrade GetTradeByMode(BlacksmithMode mode)
    {
        int targetLevel = mode == BlacksmithMode.Weapon
            ? Player.Instance.weaponLevel + 1
            : Player.Instance.armorLevel + 1;

        return trades.Find(t =>
        {
            bool correctType = mode == BlacksmithMode.Weapon
                ? IsWeapon(t.potionResult)
                : IsArmor(t.potionResult);

            return correctType && GetLevel(t.potionResult) == targetLevel;
        });
    }

    public string GetUpgradeDescription(ItemType type)
    {
        int level = GetLevel(type);

        if (IsWeapon(type))
        {
            int damage = GetWeaponDamage(level);
            return $"Mejora la espada al nivel {level}, otorgando {damage} puntos de daño.";
        }

        if (IsArmor(type))
        {
            int hp = GetArmorHP(level);
            return $"Mejora la armadura al nivel {level}, otorgando {hp} puntos de vida.";
        }

        return "";
    }

    int GetWeaponDamage(int level)
    {
        int[] values = { 0, 100, 120, 140, 170, 200, 240, 280, 350, 420, 500 };
        return level >= 0 && level < values.Length ? values[level] : 0;
    }

    int GetArmorHP(int level)
    {
        int[] values = { 0, 100, 120, 150, 190, 250, 280, 350, 400, 480, 600 };
        return level >= 0 && level < values.Length ? values[level] : 0;
    }
}