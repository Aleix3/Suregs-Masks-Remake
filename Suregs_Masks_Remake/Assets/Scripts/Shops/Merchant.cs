using UnityEngine;
using static Item;

public class Merchant : MonoBehaviour, IInteractable
{
    public int sellPrice = 10;

    PlayerEconomy economy;

    void Start()
    {
        economy = FindObjectOfType<PlayerEconomy>();
    }

    public void Interact()
    {

    }

    public void SellItem(ItemType type)
    {
        if (InventoryManager.instance.GetQuantity(type) > 0)
        {
            InventoryManager.instance.RemoveQuantity(type, 1);
            economy.AddGold(sellPrice);
        }
    }
}
