using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class InventoryItem : MonoBehaviour
{
    public ItemType type;
    int id;
    int ObjectId = -1;
    public new string name;
    public string itemType;
    public Image itemImage;
    public Image closeUpItem;
    public uint quantity = 1;
    public bool stackable = false;
    bool inList = false;
    public string description;

    private void Awake()
    {
        itemImage = GetComponent<Image>();
        closeUpItem = GetComponent<Image>();
    }
    public void AddQuantity(uint q)
    {
        quantity += q;

    }

    public void SubtractQuantity(uint q)
    {
        if (q >= quantity) quantity = 0;
        else quantity -= q;

    }


}
