using System.Collections.Generic;
using UnityEngine;
using static Item;

public class Merchant : MonoBehaviour, IShop
{
    private int pendingGold = 0;

    Dictionary<ItemType, int> pendingSell = new Dictionary<ItemType, int>();

    public event System.Action OnTradeUpdated;

    public GameObject merchantCanvas;
    public ShopUI shopUI;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            merchantCanvas.SetActive(false);
        }
    }

    public void Interact()
    {

    }

    public void SelectTrade(int num)
    {
        if (!GetTradeData(num, out ItemType type, out int goldValue))
            return;

        int currentQty = InventoryManager.instance.GetQuantity(type);
        int alreadyPending = pendingSell.ContainsKey(type) ? pendingSell[type] : 0;

        if (currentQty - alreadyPending <= 0)
        {
            for (int i = 0; i < shopUI.buttons.Count; i++)
            {
                if(shopUI.buttons[i].isSelected)
                {
                    shopUI.buttons[i].DeSelect(true);
                }
                
            }
            Debug.Log("No tienes más de este item");
            return;
        }

        if (!pendingSell.ContainsKey(type))
            pendingSell[type] = 0;

        pendingSell[type]++;
        pendingGold += goldValue;

        for (int i = 0; i < shopUI.buttons.Count; i++)
        {
            if (shopUI.buttons[i].isSelected)
            {
                shopUI.buttons[i].SelectPermanent();
            }

        }

        Debug.Log("Seleccionado 1 " + type + " | Oro acumulado: " + pendingGold);

        OnTradeUpdated?.Invoke();
    }

    public void BuyAll(int num)
    {
        if (!GetTradeData(num, out ItemType type, out int goldValue))
            return;

        int currentQty = InventoryManager.instance.GetQuantity(type);
        int alreadyPending = pendingSell.ContainsKey(type) ? pendingSell[type] : 0;

        int availableToSell = currentQty - alreadyPending;

        if (availableToSell <= 0)
        {
            for (int i = 0; i < shopUI.buttons.Count; i++)
            {
                if (shopUI.buttons[i].isSelected)
                {
                    shopUI.buttons[i].DeSelect(true);
                }

            }
            Debug.Log("No tienes más de este item para vender");
            return;
        }

        for (int i = 0; i < shopUI.buttons.Count; i++)
        {
            if (shopUI.buttons[i].isSelected)
            {
                shopUI.buttons[i].SelectPermanent();
            }

        }

        if (!pendingSell.ContainsKey(type))
            pendingSell[type] = 0;

        pendingSell[type] += availableToSell;
        pendingGold += goldValue * availableToSell;

        Debug.Log("Seleccionado TODO " + type +
                  " x" + availableToSell +
                  " | Oro acumulado: " + pendingGold);

        OnTradeUpdated?.Invoke();
    }

    public void ConfirmBuy()
    {
        foreach (var entry in pendingSell)
        {
            ItemType type = entry.Key;
            int qty = entry.Value;

            InventoryManager.instance.RemoveQuantity(type, (uint)qty);
        }

        PlayerEconomy.instance.AddGold(pendingGold);

        Debug.Log("Venta confirmada. Oro ganado: " + pendingGold);

        pendingSell.Clear();
        pendingGold = 0;

        for (int i = 0; i < shopUI.buttons.Count; i++)
        {
            shopUI.buttons[i].DeSelect();
        }

        OnTradeUpdated?.Invoke();
    }

    public void CancelTrade()
    {
        pendingSell.Clear();
        pendingGold = 0;

        for (int i = 0; i < shopUI.buttons.Count; i++)
        {
            shopUI.buttons[i].DeSelect();
        }

        OnTradeUpdated?.Invoke();

    }

    private bool GetTradeData(int num, out ItemType type, out int goldValue)
    {
        type = ItemType.RUBI;
        goldValue = 0;

        switch (num)
        {
            case 1: type = ItemType.COLA; goldValue = 10; break;
            case 2: type = ItemType.HUESO; goldValue = 15; break;
            case 3: type = ItemType.SALIVA; goldValue = 20; break;
            case 4: type = ItemType.GARRA; goldValue = 50; break;
            case 5: type = ItemType.OJO; goldValue = 5; break;
            case 6: type = ItemType.DIENTE; goldValue = 25; break;
            default: return false;
        }

        return true;
    }

    public bool TryGetGoldValue(ItemType type, out int value)
    {
        value = 0;

        switch (type)
        {
            case ItemType.COLA: value = 10; return true;
            case ItemType.HUESO: value = 15; return true;
            case ItemType.SALIVA: value = 20; return true;
            case ItemType.GARRA: value = 50; return true;
            case ItemType.OJO: value = 5; return true;
            case ItemType.DIENTE: value = 25; return true;
        }

        return false;
    }

    public int GetPending(ItemType type)
    {
        if (pendingSell.ContainsKey(type))
            return pendingSell[type];

        return 0;
    }

    public int GetRequiredItemQty(ItemType type)
    {
        return 1;
    }

    public int GetRequiredItemPending(ItemType type)
    {
        if (pendingSell.ContainsKey(type))
            return pendingSell[type];

        return 0;
    }

    public bool IsSelling() => true;
}