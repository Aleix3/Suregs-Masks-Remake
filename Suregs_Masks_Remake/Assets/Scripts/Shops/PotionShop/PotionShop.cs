using System;
using System.Collections.Generic;
using UnityEngine;
using static Item;

public class PotionShop : MonoBehaviour, IShop
{
    public List<PotionTrade> trades = new List<PotionTrade>();

    Dictionary<int, int> pendingBuy = new Dictionary<int, int>();

    public GameObject witchCanvas;

    public event Action OnTradeUpdated;

    public ShopUI shopUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            witchCanvas.SetActive(false);
            DialogueManager.Instance.CloseCommerce();
        }
            
    }

    public void Interact()
    {
        witchCanvas.SetActive(true);
    }

    public void SelectTrade(int tradeIndexFake)
    {
        int tradeIndex = tradeIndexFake - 1;
        if (tradeIndex < 0 || tradeIndex >= trades.Count)
            return;

        PotionTrade trade = trades[tradeIndex];

        // Calcula lo pendiente antes de sumar
        int alreadyPending = pendingBuy.ContainsKey(tradeIndex) ? pendingBuy[tradeIndex] : 0;

        // Comprueba oro disponible menos lo que ya está pendiente
        int goldAvailable = PlayerEconomy.instance.GetGold() - (trade.goldCost * alreadyPending);
        if (goldAvailable < trade.goldCost)
        {
            for (int i = 0; i < shopUI.buttons.Count; i++)
            {
                if (shopUI.buttons[i].isSelected)
                {
                    shopUI.buttons[i].DeSelect(true);
                }

            }
            Debug.Log("No tienes suficiente oro para añadir otra de esta poción.");
            return;
        }

        // Comprueba items requeridos menos lo pendiente
        if (trade.requiredItemQty > 0)
        {
            int qtyAvailable = InventoryManager.instance.GetQuantity(trade.requiredItem) - (trade.requiredItemQty * alreadyPending);
            if (qtyAvailable < trade.requiredItemQty)
            {
                for (int i = 0; i < shopUI.buttons.Count; i++)
                {
                    if (shopUI.buttons[i].isSelected)
                    {
                        shopUI.buttons[i].DeSelect(true);
                    }

                }
                Debug.Log("No tienes suficientes items para añadir otra de esta poción.");
                return;
            }
        }
        for (int i = 0; i < shopUI.buttons.Count; i++)
        {
            if (shopUI.buttons[i].isSelected)
            {
                shopUI.buttons[i].SelectPermanent();
            }

        }
        // Si pasa los checks, suma 1 al pending
        if (!pendingBuy.ContainsKey(tradeIndex))
            pendingBuy[tradeIndex] = 0;

        pendingBuy[tradeIndex]++;

        Debug.Log("Añadida poción: " + trade.potionResult + " | Pendiente ahora: " + pendingBuy[tradeIndex]);

        OnTradeUpdated?.Invoke();
    }

    public void BuyAll(int tradeIndexFake)
    {
        int tradeIndex = tradeIndexFake - 1;
        if (tradeIndex < 0 || tradeIndex >= trades.Count)
            return;

        PotionTrade trade = trades[tradeIndex];

        // Calcula lo pendiente
        int alreadyPending = pendingBuy.ContainsKey(tradeIndex) ? pendingBuy[tradeIndex] : 0;

        // Oro disponible teniendo en cuenta lo pendiente
        int goldAvailable = PlayerEconomy.instance.GetGold() - (trade.goldCost * alreadyPending);

        // Items disponibles si hace falta algún item
        int itemAvailable = 0;
        if (trade.requiredItemQty > 0)
            itemAvailable = InventoryManager.instance.GetQuantity(trade.requiredItem) - (trade.requiredItemQty * alreadyPending);

        // Calcula cuántas pociones puedes comprar con oro
        int maxByGold = goldAvailable / trade.goldCost;

        // Calcula cuántas pociones puedes comprar con items (si aplica)
        int maxByItem = trade.requiredItemQty > 0 ? itemAvailable / trade.requiredItemQty : int.MaxValue;

        // Cuántas se pueden comprar realmente
        int canBuy = Mathf.Min(maxByGold, maxByItem);

        if (canBuy <= 0)
        {
            for (int i = 0; i < shopUI.buttons.Count; i++)
            {
                if (shopUI.buttons[i].isSelected)
                {
                    shopUI.buttons[i].DeSelect(true);
                }

            }
            Debug.Log("No tienes suficiente oro o items para comprar más pociones.");
            return;
        }

        for (int i = 0; i < shopUI.buttons.Count; i++)
        {
            if (shopUI.buttons[i].isSelected)
            {
                shopUI.buttons[i].SelectPermanent();
            }

        }

        if (!pendingBuy.ContainsKey(tradeIndex))
            pendingBuy[tradeIndex] = 0;

        pendingBuy[tradeIndex] += canBuy;

        Debug.Log($"Seleccionado TODO {trade.potionResult} x{canBuy} | Pendiente ahora: {pendingBuy[tradeIndex]}");

        OnTradeUpdated?.Invoke();
    }

    public void ConfirmBuy()
    {
        foreach (var entry in pendingBuy)
        {
            PotionTrade trade = trades[entry.Key];
            int qty = entry.Value;

            int totalGold = trade.goldCost * qty;

            if (PlayerEconomy.instance.GetGold() < totalGold)
                continue;

            PlayerEconomy.instance.AddGold(-totalGold);

            if (trade.requiredItemQty > 0)
            {
                InventoryManager.instance.RemoveQuantity(
                    trade.requiredItem,
                    (uint)(trade.requiredItemQty * qty)
                );
            }

            InventoryManager.instance.AddItem(trade.potionResult, (uint)qty);
        }
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

    public int GetPending(ItemType type)
    {
        int tradeIndex = trades.FindIndex(t => t.potionResult == type);
        Debug.Log($"GetPending llamado para {type}, tradeIndex encontrado: {tradeIndex}");

        if (tradeIndex == -1)
            return 0;

        if (pendingBuy.ContainsKey(tradeIndex))
        {
            Debug.Log($"Cantidad pendiente para {type}: {pendingBuy[tradeIndex]}");
            return pendingBuy[tradeIndex];
        }

        Debug.Log($"No hay cantidad pendiente para {type}");
        return 0;
    }

    public int GetRequiredItemQty(ItemType type)
    {
        int tradeIndex = trades.FindIndex(t => t.potionResult == type);
        Debug.Log($"GetPending llamado para {type}, tradeIndex encontrado: {tradeIndex}");

        if (tradeIndex == -1)
            return 0;

            return trades[tradeIndex].requiredItemQty;


    }

    public bool TryGetGoldValue(ItemType type, out int value)
    {
        for (int i = 0; i < trades.Count; i++)
        {
            if (trades[i].potionResult == type)
            {
                value = trades[i].goldCost;
                return true;
            }
        }

        value = 0;
        return false;
    }

    public int GetRequiredItemPending(ItemType type)
    {
        int total = 0;

        for (int i = 0; i < trades.Count; i++)
        {
            PotionTrade trade = trades[i];

            if (trade.requiredItem != type)
                continue;

            int pending = pendingBuy.ContainsKey(i) ? pendingBuy[i] : 0;

            total += pending * trade.requiredItemQty;
        }

        return total;
    }

    public bool IsSelling() => false;
}