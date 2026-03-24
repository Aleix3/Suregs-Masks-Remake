using System.Collections.Generic;
using UnityEngine;
using static Item;



public class BlacksmithShop : MonoBehaviour, IInteractable
{
    public List<BlackSmithTrade> trades = new List<BlackSmithTrade>();
    public GameObject canvas;

    // Controla cuántos trades están pendientes de confirmar
    private Dictionary<int, int> pendingBuy = new Dictionary<int, int>();

    public enum BlacksmithMode
    {
        Weapon,
        Armor
    }

    public BlacksmithMode currentMode;

    public void Interact()
    {
        canvas.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            canvas.SetActive(false);
    }

    // 🔹 Lógica para seleccionar un trade
    public void SelectTrade(int tradeIndexFake)
    {
        int index = tradeIndexFake - 1;

        if (index < 0 || index >= trades.Count)
            return;

        BlackSmithTrade trade = trades[index];

        int alreadyPending = pendingBuy.ContainsKey(index) ? pendingBuy[index] : 0;

        // Chequeo de oro disponible
        int goldAvailable = PlayerEconomy.instance.GetGold() - (trade.goldCost * alreadyPending);
        if (goldAvailable < trade.goldCost)
        {
            Debug.Log("No tienes suficiente oro");
            return;
        }

        // Chequeo de items requeridos
        if (trade.requiredItemQty > 0)
        {
            int qtyAvailable = InventoryManager.instance.GetQuantity(trade.requiredItem)
                             - (trade.requiredItemQty * alreadyPending);

            if (qtyAvailable < trade.requiredItemQty)
            {
                Debug.Log("No tienes suficientes materiales");
                return;
            }
        }

        // Sumamos al pending
        if (!pendingBuy.ContainsKey(index))
            pendingBuy[index] = 0;

        pendingBuy[index]++;
        Debug.Log($"Seleccionado trade {trade.potionResult} | Pendiente: {pendingBuy[index]}");
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
        }

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

    // 🔹 Obtiene el nivel del item a partir de su nombre (ESPADA_NV4 → 4)
    private int GetLevel(ItemType type)
    {
        string name = type.ToString();
        int lastUnderscore = name.LastIndexOf('_');
        if (lastUnderscore == -1)
            return 0;

        string numberStr = name.Substring(lastUnderscore + 1);
        if (int.TryParse(numberStr, out int level))
            return level;

        return 0;
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

        // Actualizar UI
    }
}