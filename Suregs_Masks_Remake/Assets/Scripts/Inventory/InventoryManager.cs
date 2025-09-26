using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;
using UnityEngine.UI;
using static UnityEditor.Progress;
public class InventoryManager : MonoBehaviour
{

    public static InventoryManager instance { get; private set; }

    public List<InventoryItem> inventoryItems = new List<InventoryItem>();

    public GameObject itemPrefab;

    public Canvas inventoryCanvas;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // evita duplicados
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // opcional: persiste entre escenas
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void createInventoryItem(ItemType type, string itemType, string name, string description, Sprite itemImage)
    {

        GameObject newItem = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        InventoryItem item = newItem.GetComponent<InventoryItem>();
        item.type = type;
        item.description = description;
        item.name = name;
        item.itemImage.sprite = itemImage;
        item.closeUpItem.sprite = itemImage;
        item.itemType = itemType;
        newItem.transform.SetParent(inventoryCanvas.transform, worldPositionStays: false);
        AddInventoryItem(item);

    }

    void AddInventoryItem(InventoryItem item)
    {
        bool alreadyHaveItem = false;
        for (int i = 0; i < inventoryItems.Count; i++) {
            if (inventoryItems[i].type == item.type)
            {
                inventoryItems[i].quantity += 1;
                alreadyHaveItem = true;
            }

        }

        if (!alreadyHaveItem)
        {
            inventoryItems.Add(item);
        }


        
    }

    private void DestroyInventoryItem(ItemType type, uint quantity)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].type == type)
            {
                if (inventoryItems[i].quantity > quantity)
                {
                    inventoryItems[i].quantity -= quantity;
                }
                else
                {
                    inventoryItems.RemoveAt(i);
                }
            }

        }
    }
}
