using System.Collections.Generic;
using System.Data;
using UnityEngine;
using static Item;

public class Merchant : MonoBehaviour, IInteractable
{
    private int pendingGold = 0;
    Dictionary<ItemType, int> pendingSell = new Dictionary<ItemType, int>();
    public System.Action OnTradeUpdated;
    public GameObject merchantCanvas;

    void Start()
    {

    }

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
            Debug.Log("No tienes más de este item");
            return;
        }

        if (!pendingSell.ContainsKey(type))
            pendingSell[type] = 0;

        pendingSell[type]++;
        pendingGold += goldValue;

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
        OnTradeUpdated?.Invoke();
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
        OnTradeUpdated?.Invoke();
    }

    public void CancelTrade()
    {
        pendingSell.Clear();
        pendingGold = 0;
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
            case ItemType.RUBI: value = 10; return true;
            case ItemType.SALIVA: value = 15; return true;
            case ItemType.AMATISTA: value = 20; return true;
            case ItemType.PEZ_PEQUENO: value = 50; return true;
            case ItemType.POLVORA: value = 5; return true;
            case ItemType.HUESO: value = 25; return true;
        }

        return false;
    }

    public int GetPending(ItemType type)
    {
        if (pendingSell.ContainsKey(type))
            return pendingSell[type];

        return 0;
    }
}
