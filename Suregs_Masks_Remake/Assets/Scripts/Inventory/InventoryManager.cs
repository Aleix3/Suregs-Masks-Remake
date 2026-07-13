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
    public TextMeshProUGUI inventoryQuantity;
    public Image inventoryCloseUpImage;

    public GameObject inventorySlots;
    public GameObject hover;
    public int currentIndex = 0;
    private int rows = 4;
    private int cols = 3;

    public event System.Action OnInventoryChanged;

    [SerializeField] private CanvasGroup inventoryGroup;
    private bool isInventoryOpen;

    [System.Serializable]
    public class InventorySaveData
    {
        public Item.ItemType type;
        public uint quantity;
    }

    [System.Serializable]
    public class InventorySave
    {
        public List<InventorySaveData> items = new();
    }


    //public GameObject merchantCanvas;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); // evita duplicados
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        LoadInventory();
    }

    private void Start()
    {
        if (inventorySlots.transform.childCount > 0)
            MoveHoverTo(currentIndex);
        hover.transform.localScale = new Vector3(0.662f, 0.662f, 0.662f);

        
        
    }

    private void Update()
    {



        if (inventoryCanvas.gameObject.activeSelf == false) return;
        if (inventorySlots.transform.childCount == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            currentIndex++;
            if (currentIndex >= inventorySlots.transform.childCount)
                currentIndex = 0;
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = inventorySlots.transform.childCount - 1;
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentIndex += cols;
            if (currentIndex >= inventorySlots.transform.childCount)
                currentIndex %= cols;
            MoveHoverTo(currentIndex);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentIndex -= cols;
            if (currentIndex < 0)
                currentIndex += inventorySlots.transform.childCount;
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
            inventoryCloseUpImage.enabled = true;
            inventoryName.text = item.name;
            inventoryDescription.text = item.description;
            inventoryQuantity.text = "X" + item.quantity;
            inventoryCloseUpImage.sprite = item.closeUpItem.sprite;
        }
        else
        {
            inventoryName.text = "";
            inventoryDescription.text = "";
            inventoryQuantity.text = "";
            inventoryCloseUpImage.enabled = false;
        }
    }

    public InventoryItem CreateInventoryItem(ItemType type, string itemType, string name, string description, Sprite itemSprite, uint quantity = 1)
    {
        // buscar si ya existe (comparando type + itemType)
        InventoryItem found = inventoryItems.Find(i => i.type == type && i.itemType == itemType);
        if (found != null)
        {
            found.AddQuantity(quantity);
            SaveInventory();
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

            if (slot.GetComponentInChildren<InventoryItem>() == null)
            {
                newItem.transform.SetParent(slot, false);
                newItem.transform.localPosition = Vector3.zero;
                break;
            }
        }
        
        inventoryItems.Add(itemComp);
        SaveInventory();
        MoveHoverTo(currentIndex);
        OnInventoryChanged?.Invoke();
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
                    
                }
                else
                {
                    // quitar por completo
                    Destroy(it.gameObject);
                    inventoryItems.RemoveAt(i);

                }
                ReorderInventory();
                OnInventoryChanged?.Invoke();
                SaveInventory();
                return true;
            }
        }
        
        return false;
    }

    public InventoryItem AddItem(Item.ItemType type, uint quantity = 1)
    {
        
        Item.GetItemData(type, out string name, out string description, out string itemType, out Sprite sprite);
        OnInventoryChanged?.Invoke();
        return CreateInventoryItem(
            type,
            itemType,
            name,
            description,
            sprite,
            quantity
        );

    }

    private void ReorderInventory()
    {
        // SOLO recolocar, NO destruir nada
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            InventoryItem item = inventoryItems[i];

            if (item == null) continue;

            Transform slot = inventorySlots.transform.GetChild(i);

            item.transform.SetParent(slot, false);
            item.transform.localPosition = Vector3.zero;
        }

        // limpiar slots sobrantes
        for (int i = inventoryItems.Count; i < inventorySlots.transform.childCount; i++)
        {
            Transform slot = inventorySlots.transform.GetChild(i);

            foreach (Transform child in slot)
            {
                Destroy(child.gameObject);
            }
        }

        // ajustar hover
        if (inventoryItems.Count == 0)
        {
            currentIndex = 0;
            return;
        }

        if (currentIndex >= inventoryItems.Count)
            currentIndex = inventoryItems.Count - 1;

        MoveHoverTo(currentIndex);
    }

    private void SetInventoryVisible(bool visible)
    {
        inventoryGroup.alpha = visible ? 1 : 0;
        inventoryGroup.interactable = visible;
        inventoryGroup.blocksRaycasts = visible;
    }

    private const string INVENTORY_KEY = "Inventory";

    public void SaveInventory()
    {
        InventorySave save = new InventorySave();

        foreach (InventoryItem item in inventoryItems)
        {
            save.items.Add(new InventorySaveData
            {
                type = item.type,
                quantity = item.quantity
            });
        }

        string json = JsonUtility.ToJson(save);
        PlayerPrefs.SetString(INVENTORY_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        if (!PlayerPrefs.HasKey(INVENTORY_KEY))
            return;

        ClearInventory();

        string json = PlayerPrefs.GetString(INVENTORY_KEY);

        InventorySave save = JsonUtility.FromJson<InventorySave>(json);

        if (save == null)
            return;

        foreach (var item in save.items)
        {
            AddItem(item.type, item.quantity);
        }

        ReorderInventory();
    }

    public void ClearInventory()
    {
        foreach (InventoryItem item in inventoryItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        inventoryItems.Clear();

        foreach (Transform slot in inventorySlots.transform)
        {
            foreach (Transform child in slot)
                Destroy(child.gameObject);
        }

        currentIndex = 0;
    }
}
