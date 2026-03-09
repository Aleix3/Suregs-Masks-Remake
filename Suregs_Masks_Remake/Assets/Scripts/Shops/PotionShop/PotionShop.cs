using System;
using System.Collections.Generic;
using UnityEngine;
using static Item;

public class PotionShop : MonoBehaviour, IInteractable, IShop
{
    public List<PotionTrade> trades = new List<PotionTrade>();

    Dictionary<int, int> pendingBuy = new Dictionary<int, int>();

    public GameObject witchCanvas;

    private Action _onTradeUpdated;

    public Action OnTradeUpdated => _onTradeUpdated;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            witchCanvas.SetActive(false);
    }

    public void Interact()
    {
        witchCanvas.SetActive(true);
    }

    public void SelectTrade(int tradeIndex)
    {
        if (tradeIndex < 0 || tradeIndex >= trades.Count)
            return;

        PotionTrade trade = trades[tradeIndex];

        int gold = PlayerEconomy.instance.GetGold();

        if (gold < trade.goldCost)
        {
            Debug.Log("No tienes suficiente oro");
            return;
        }

        if (trade.requiredItemQty > 0)
        {
            int qty = InventoryManager.instance.GetQuantity(trade.requiredItem);

            if (qty <= 0)
            {
                Debug.Log("Falta item requerido");
                return;
            }
        }

        if (!pendingBuy.ContainsKey(tradeIndex))
            pendingBuy[tradeIndex] = 0;

        pendingBuy[tradeIndex]++;

        Debug.Log("Añadida poción: " + trade.potionResult);

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

        pendingBuy.Clear();

        OnTradeUpdated?.Invoke();
    }

    public void CancelTrade()
    {
        pendingBuy.Clear();
        OnTradeUpdated?.Invoke();
    }

    public int GetPending(ItemType type)
    {
        for (int i = 0; i < trades.Count; i++)
        {
            if (trades[i].potionResult == type)
            {
                if (pendingBuy.ContainsKey(i))
                    return pendingBuy[i];

                return 0;
            }
        }

        return 0;
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
}