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

    [Header("Descuento Amatista")]
    [Tooltip("Nombre de itemType que, si está en el inventario, aplica el descuento del 50%.")]
    public string discountItemType = "AMATISTA";

    private bool HasDiscount()
    {
        return InventoryManager.instance != null && InventoryManager.instance.HasItem(ItemType.AMATISTA);
    }

    private int GetEffectiveGoldCost(PotionTrade trade)
    {
        if (!HasDiscount()) return trade.goldCost;
        return Mathf.Max(1, trade.goldCost / 2);
    }

    private int GetEffectiveRequiredItemQty(PotionTrade trade)
    {
        // 0 significa "no requiere item", eso no se toca
        if (trade.requiredItemQty <= 0) return trade.requiredItemQty;
        if (!HasDiscount()) return trade.requiredItemQty;
        return Mathf.Max(1, trade.requiredItemQty / 2);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            witchCanvas.SetActive(false);
            DialogueManager.Instance.CloseCommerce();
        }

    }

    private void OnEnable()
    {
        QuestManager.Instance.CompleteMainStepById("8");
        QuestManager.Instance.CompleteMainStepById("23");
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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        PotionTrade trade = trades[tradeIndex];


        int effectiveGoldCost = GetEffectiveGoldCost(trade);
        int effectiveItemQty = GetEffectiveRequiredItemQty(trade);
        // Calcula lo pendiente antes de sumar
        int alreadyPending = pendingBuy.ContainsKey(tradeIndex) ? pendingBuy[tradeIndex] : 0;

        // Comprueba oro disponible menos lo que ya est� pendiente
        int goldAvailable = PlayerEconomy.instance.GetGold() - (effectiveGoldCost * alreadyPending);
        if (goldAvailable < effectiveGoldCost)
        {
            for (int i = 0; i < shopUI.buttons.Count; i++)
            {
                if (shopUI.buttons[i].isSelected)
                {
                    shopUI.buttons[i].DeSelect(true);
                }

            }
            Debug.Log("No tienes suficiente oro para a�adir otra de esta poci�n.");
            return;
        }

        // Comprueba items requeridos menos lo pendiente
        if (trade.requiredItemQty > 0)
        {
            int qtyAvailable = InventoryManager.instance.GetQuantity(trade.requiredItem) - (effectiveItemQty * alreadyPending);
            if (qtyAvailable < effectiveItemQty)
            {
                for (int i = 0; i < shopUI.buttons.Count; i++)
                {
                    if (shopUI.buttons[i].isSelected)
                    {
                        shopUI.buttons[i].DeSelect(true);
                    }

                }
                Debug.Log("No tienes suficientes items para a�adir otra de esta poci�n.");
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

        Debug.Log("A�adida poci�n: " + trade.potionResult + " | Pendiente ahora: " + pendingBuy[tradeIndex]);

        OnTradeUpdated?.Invoke();
    }

    public void BuyAll(int tradeIndexFake)
    {
        int tradeIndex = tradeIndexFake - 1;
        if (tradeIndex < 0 || tradeIndex >= trades.Count)
            return;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        PotionTrade trade = trades[tradeIndex];

        int effectiveGoldCost = GetEffectiveGoldCost(trade);
        int effectiveItemQty = GetEffectiveRequiredItemQty(trade);

        // Calcula lo pendiente
        int alreadyPending = pendingBuy.ContainsKey(tradeIndex) ? pendingBuy[tradeIndex] : 0;

        // Oro disponible teniendo en cuenta lo pendiente
        int goldAvailable = PlayerEconomy.instance.GetGold() - (effectiveGoldCost * alreadyPending);

        // Items disponibles si hace falta alg�n item
        int itemAvailable = 0;
        if (trade.requiredItemQty > 0)
            itemAvailable = InventoryManager.instance.GetQuantity(trade.requiredItem) - (effectiveItemQty * alreadyPending);

        // Calcula cu�ntas pociones puedes comprar con oro
        int maxByGold = goldAvailable / effectiveGoldCost;

        // Calcula cu�ntas pociones puedes comprar con items (si aplica)
        int maxByItem = trade.requiredItemQty > 0 ? itemAvailable / effectiveItemQty : int.MaxValue;

        // Cu�ntas se pueden comprar realmente
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
            Debug.Log("No tienes suficiente oro o items para comprar m�s pociones.");
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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        foreach (var entry in pendingBuy)
        {
            PotionTrade trade = trades[entry.Key];
            int effectiveGoldCost = GetEffectiveGoldCost(trade);
            int effectiveItemQty = GetEffectiveRequiredItemQty(trade);
            int qty = entry.Value;

            int totalGold = effectiveGoldCost * qty;

            if (PlayerEconomy.instance.GetGold() < totalGold)
                continue;

            PlayerEconomy.instance.AddGold(-totalGold);

            if (trade.requiredItemQty > 0)
            {
                InventoryManager.instance.RemoveQuantity(
                    trade.requiredItem,
                    (uint)(effectiveItemQty * qty)
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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
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

        return GetEffectiveRequiredItemQty(trades[tradeIndex]);


    }

    public bool TryGetGoldValue(ItemType type, out int value)
    {
        for (int i = 0; i < trades.Count; i++)
        {
            if (trades[i].potionResult == type)
            {
                value = GetEffectiveGoldCost(trades[i]);
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

            total += pending * GetEffectiveRequiredItemQty(trade);
        }

        return total;
    }

    public bool IsSelling() => false;
}