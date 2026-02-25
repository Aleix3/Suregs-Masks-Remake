using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static Item;

public class Merchant : MonoBehaviour, IInteractable
{
    private int pendingGold = 0;
    Dictionary<ItemType, int> pendingSell = new Dictionary<ItemType, int>();

    void Start()
    {

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
            Debug.Log("No tienes más de este item");
            return;
        }

        if (!pendingSell.ContainsKey(type))
            pendingSell[type] = 0;

        pendingSell[type]++;
        pendingGold += goldValue;

        Debug.Log("Seleccionado 1 " + type + " | Oro acumulado: " + pendingGold);
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
            Debug.Log("No tienes más de este item para vender");
            return;
        }

        if (!pendingSell.ContainsKey(type))
            pendingSell[type] = 0;

        pendingSell[type] += availableToSell;
        pendingGold += goldValue * availableToSell;

        Debug.Log("Seleccionado TODO " + type +
                  " x" + availableToSell +
                  " | Oro acumulado: " + pendingGold);
    }

    public void ConfirmBuy()
    {
        // quitar items
        foreach (var entry in pendingSell)
        {
            ItemType type = entry.Key;
            int qty = entry.Value;

            InventoryManager.instance.RemoveQuantity(type, (uint)qty);
        }

        // añadir oro
        PlayerEconomy.instance.AddGold(pendingGold);

        Debug.Log("Venta confirmada. Oro ganado: " + pendingGold);

        // limpiar acumuladores
        pendingSell.Clear();
        pendingGold = 0;
    }

    public void CancelTrade()
    {
        pendingSell.Clear();
        pendingGold = 0;
    }

    private bool GetTradeData(int num, out ItemType type, out int goldValue)
    {
        type = ItemType.RUBI;
        goldValue = 0;

        switch (num)
        {
            case 1: type = ItemType.RUBI; goldValue = 10; break;
            case 2: type = ItemType.SALIVA; goldValue = 15; break;
            case 3: type = ItemType.AMATISTA; goldValue = 20; break;
            case 4: type = ItemType.PEZ_PEQUENO; goldValue = 50; break;
            case 5: type = ItemType.POLVORA; goldValue = 5; break;
            case 6: type = ItemType.HUESO; goldValue = 25; break;
            default: return false;
        }

        return true;
    }
}
