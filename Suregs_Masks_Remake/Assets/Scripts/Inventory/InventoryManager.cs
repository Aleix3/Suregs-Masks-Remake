using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Item;
using UnityEngine.UI;
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
    private bool isLoadingInventory;

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
        



    }

    private void Update()
    {
        if (MenuManager.Instance.currentIndex != 0)
        {
            return;
        }
        if (inventoryCanvas.gameObject.activeSelf == false) return;
        if (inventorySlots.transform.childCount == 0) return;

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            currentIndex++;
            if (currentIndex >= inventorySlots.transform.childCount)
                currentIndex = 0;
            MoveHoverTo(currentIndex);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = inventorySlots.transform.childCount - 1;
            MoveHoverTo(currentIndex);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            currentIndex += cols;
            if (currentIndex >= inventorySlots.transform.childCount)
                currentIndex %= cols;
            MoveHoverTo(currentIndex);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            currentIndex -= cols;
            if (currentIndex < 0)
                currentIndex += inventorySlots.transform.childCount;
            MoveHoverTo(currentIndex);
        }
    }

    private void MoveHoverTo(int index)
    {
        if (inventorySlots == null)
            return;

        if (hover == null)
            return;

        if (inventorySlots.transform.childCount == 0)
            return;

        if (index < 0 || index >= inventorySlots.transform.childCount)
            return;

        if (AudioManager.Instance != null &&
            AudioManager.Instance.changeInventoryWindowClip != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.changeInventoryWindowClip
            );
        }

        Transform slot = inventorySlots.transform.GetChild(index);

        hover.transform.SetParent(slot, false);
        hover.transform.localPosition = Vector3.zero;

        InventoryItem item = slot.GetComponentInChildren<InventoryItem>();

        if (item != null)
        {
            inventoryCloseUpImage.enabled = true;
            inventoryName.text = item.name;
            inventoryDescription.text = item.description;
            inventoryQuantity.text = "X" + item.quantity;

            if (item.closeUpItem != null)
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

    public InventoryItem CreateInventoryItem(
    ItemType type,
    string itemType,
    string name,
    string description,
    Sprite itemSprite,
    uint quantity = 1)
    {
        InventoryItem found = inventoryItems.Find(
            i => i.type == type && i.itemType == itemType
        );

        if (found != null)
        {
            found.AddQuantity(quantity);

            if (!isLoadingInventory)
            {
                SaveInventory();
                OnInventoryChanged?.Invoke();
            }

            return found;
        }

        Transform emptySlot = null;

        for (int s = 0; s < inventorySlots.transform.childCount; s++)
        {
            Transform slot = inventorySlots.transform.GetChild(s);

            if (slot.GetComponentInChildren<InventoryItem>() == null)
            {
                emptySlot = slot;
                break;
            }
        }

        if (emptySlot == null)
        {
            Debug.Log("Inventario lleno. No se puede añadir el item: " + name);
            return null;
        }

        GameObject newItem = Instantiate(itemPrefab);
        InventoryItem itemComp = newItem.GetComponent<InventoryItem>();

        itemComp.type = type;
        itemComp.itemType = itemType;
        itemComp.name = name;
        itemComp.description = description;
        itemComp.quantity = quantity;

        if (itemComp.itemImage != null)
            itemComp.itemImage.sprite = itemSprite;

        if (itemComp.closeUpItem != null)
            itemComp.closeUpItem.sprite = itemSprite;

        itemComp.transform.SetParent(emptySlot, false);
        itemComp.transform.localPosition = Vector3.zero;

        inventoryItems.Add(itemComp);

        if (!isLoadingInventory)
        {
            SaveInventory();
            MoveHoverTo(currentIndex);
            OnInventoryChanged?.Invoke();
        }

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
        Item.GetItemData(
            type,
            out string name,
            out string description,
            out string itemType,
            out Sprite sprite
        );

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
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            InventoryItem item = inventoryItems[i];

            if (item == null) continue;

            if (i >= inventorySlots.transform.childCount)
            {
                Debug.LogError(
                    $"No existe el slot {i}. " +
                    $"Items: {inventoryItems.Count}, " +
                    $"Slots: {inventorySlots.transform.childCount}"
                );
                break;
            }

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
                if (child.gameObject == hover)
                    continue;

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

    public bool HasItem(ItemType itemType)
    {
        return inventoryItems.Exists(i => i.type == itemType);
    }

    public void LoadInventory()
    {
        if (!PlayerPrefs.HasKey(INVENTORY_KEY))
            return;

        isLoadingInventory = true;

        ClearInventory();

        string json = PlayerPrefs.GetString(INVENTORY_KEY);
        InventorySave save = JsonUtility.FromJson<InventorySave>(json);

        if (save == null)
        {
            isLoadingInventory = false;
            return;
        }

        foreach (var item in save.items)
        {
            AddItem(item.type, item.quantity);
        }

        isLoadingInventory = false;

        ReorderInventory();

        OnInventoryChanged?.Invoke();
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
            {
                if (child.gameObject == hover)
                    continue;

                Destroy(child.gameObject);
            }
        }

        currentIndex = 0;
    }

    public void FirstHoverMove()
    {
        if (inventorySlots.transform.childCount > 0)
            MoveHoverTo(currentIndex);
        hover.transform.localScale = new Vector3(0.662f, 0.662f, 0.662f);
    }


    public bool IsInventoryFull()
    {
        Transform emptySlot = null;

        for (int s = 0; s < inventorySlots.transform.childCount; s++)
        {
            Transform slot = inventorySlots.transform.GetChild(s);

            if (slot.GetComponentInChildren<InventoryItem>() == null)
            {
                emptySlot = slot;
                break;
            }
        }

        // 3. Inventario lleno.
        if (emptySlot == null)
        {
            return true;
        }

        return false;
    }
}