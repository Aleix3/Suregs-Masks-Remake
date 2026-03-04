using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;
using UnityEngine.UI;
using static UnityEditor.Progress;
using TMPro;
using static Enemy;
public class InventoryManager : MonoBehaviour
{

    public static InventoryManager instance { get; private set; }

    public List<InventoryItem> inventoryItems = new List<InventoryItem>();

    public GameObject itemPrefab;

    public Canvas inventoryCanvas;
    public TextMeshProUGUI inventoryName;
    public TextMeshProUGUI inventoryDescription;
    public Image inventoryCloseUpImage;

    public GameObject inventorySlots;
    public GameObject hover;
    public int currentIndex = 0;
    private int rows = 4;
    private int cols = 3;

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

    private void Start()
    {
        if (inventorySlots.transform.childCount > 0)
            MoveHoverTo(currentIndex);
        hover.transform.localScale = new Vector3(0.662f, 0.662f, 0.662f);

        
        
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            inventoryCanvas.gameObject.SetActive(!inventoryCanvas.gameObject.activeSelf);
        }

        if (inventoryCanvas.gameObject.activeSelf == false) return;
        if (inventorySlots.transform.childCount == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            if (currentIndex % cols == 0) // pasa del borde derecho
                currentIndex -= cols;     // vuelve al principio de la fila
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0 || currentIndex % cols == cols - 1) // pasa del borde izq
                currentIndex += cols;  // salta al final de la fila
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex += cols;
            if (currentIndex >= rows * cols) // pasa del borde inferior
                currentIndex %= cols;       // vuelve a la fila superior misma columna
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex -= cols;
            if (currentIndex < 0)           // pasa del borde superior
                currentIndex += rows * cols; // baja a la última fila misma columna
            MoveHoverTo(currentIndex);
        }
    }

    private void MoveHoverTo(int index)
    {
        Transform slot = inventorySlots.transform.GetChild(index);
        hover.transform.SetParent(slot, false);
        hover.transform.localPosition = Vector3.zero;

        // Buscar si hay un hermano del hover item en este slot
        InventoryItem item = slot.GetComponentInChildren<InventoryItem>();
        if (item != null)
        {
            inventoryName.text = item.name;
            inventoryDescription.text = item.description;
            inventoryCloseUpImage.sprite = item.closeUpItem.sprite;
        }
        else
        {
            inventoryName.text = "";
            inventoryDescription.text = "";
            inventoryCloseUpImage.sprite = null;
        }
    }

    public InventoryItem CreateInventoryItem(ItemType type, string itemType, string name, string description, Sprite itemSprite, uint quantity = 1)
    {
        // buscar si ya existe (comparando type + itemType)
        InventoryItem found = inventoryItems.Find(i => i.type == type && i.itemType == itemType);
        if (found != null)
        {
            found.AddQuantity(quantity);
            return found;
        }

        // crear nuevo GameObject en el primer slot vacío
        GameObject newItem = Instantiate(itemPrefab);
        InventoryItem itemComp = newItem.GetComponent<InventoryItem>();
        itemComp.type = type;
        itemComp.itemType = itemType;
        itemComp.name = name;
        itemComp.description = description;
        itemComp.quantity = quantity;

        if (itemComp.itemImage != null) itemComp.itemImage.sprite = itemSprite;
        if (itemComp.closeUpItem != null) itemComp.closeUpItem.sprite = itemSprite;

        // buscar slot vacío
        for (int s = 0; s < inventorySlots.transform.childCount; s++)
        {
            Transform slot = inventorySlots.transform.GetChild(s);
            if (slot.childCount == 0)
            {
                newItem.transform.SetParent(slot, false);
                newItem.transform.localPosition = Vector3.zero;
                break;
            }
        }

        inventoryItems.Add(itemComp);
        return itemComp;
    }

    public int GetQuantity(ItemType type)
    {
        int total = 0;
        foreach (var it in inventoryItems)
            if (it.type == type)
                total += (int)it.quantity;
        return total;
    }

    public bool RemoveQuantity(ItemType type, uint quantity)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            var it = inventoryItems[i];
            if (it.type == type)
            {
                if (it.quantity > quantity)
                {
                    it.SubtractQuantity(quantity);
                    return true;
                }
                else
                {
                    // quitar por completo
                    Destroy(it.gameObject);
                    inventoryItems.RemoveAt(i);
                    return true;
                }
            }
        }
        return false;
    }

    //void AddInventoryItem(InventoryItem item)
    //{
    //    bool alreadyHaveItem = false;
    //    for (int i = 0; i < inventoryItems.Count; i++) {
    //        if (inventoryItems[i].type == item.type)
    //        {
    //            inventoryItems[i].quantity += 1;
    //            alreadyHaveItem = true;
    //        }

    //    }

    //    if (!alreadyHaveItem)
    //    {
    //        inventoryItems.Add(item);
    //        for (int i = inventorySlots.transform.childCount - 1; i >= 0; i--)
    //        {
    //            Transform slot = inventorySlots.transform.GetChild(i);

    //            if (slot.childCount == 0)
    //            {
    //                item.transform.SetParent(slot.transform, worldPositionStays: false);
    //                item.transform.localPosition = Vector3.zero;
    //            }

    //        }

    //    }

    //}

    //public void DestroyInventoryItem(ItemType type, uint quantity)
    //{
    //    for (int i = 0; i < inventoryItems.Count; i++)
    //    {
    //        if (inventoryItems[i].type == type)
    //        {
    //            if (inventoryItems[i].quantity > quantity)
    //            {
    //                inventoryItems[i].quantity -= quantity;
    //            }
    //            else
    //            {
    //                inventoryItems.RemoveAt(i);
    //            }
    //        }

    //    }
    //}
}
