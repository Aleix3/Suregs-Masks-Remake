using UnityEngine;
using TMPro;
using static Item;

public class TradeButtonUI : MonoBehaviour
{
    public MonoBehaviour shopBehaviour;
    private IShop shop;

    public ItemType itemType;
    public ItemType requiredItemType;

    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI pendingText;
    public TextMeshProUGUI goldValueText;
    public TextMeshProUGUI requiredItemQtyText;
    public TextMeshProUGUI itemQtyText;

    private void Start()
    {
        shop = shopBehaviour as IShop;

        if (shop == null)
        {
            Debug.LogError("El objeto asignado no implementa IShop");
            return;
        }

        shop.OnTradeUpdated += Refresh;

        if (InventoryManager.instance != null)
            InventoryManager.instance.OnInventoryChanged -= Refresh;

        if (PlayerEconomy.instance != null)
            PlayerEconomy.instance.OnGoldChanged -= Refresh;

        Refresh();
    }

    private void OnEnable()
    {
        shop = shopBehaviour as IShop;

        if (shop == null)
        {
            Debug.LogError("El objeto no implementa IShop");
            return;
        }

        shop.OnTradeUpdated += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (shop != null)
            shop.OnTradeUpdated -= Refresh;

        if (InventoryManager.instance != null)
            InventoryManager.instance.OnInventoryChanged -= Refresh;

        if (PlayerEconomy.instance != null)
            PlayerEconomy.instance.OnGoldChanged -= Refresh;
    }

    void Refresh()
    {
        if (InventoryManager.instance == null)
            return;

        int currentQty = InventoryManager.instance.GetQuantity(itemType);
        int pendingQty = shop.GetPending(itemType);

        int currentRequiredItems = InventoryManager.instance.GetQuantity(requiredItemType);
        int requiredPendingQty = shop.GetRequiredItemPending(requiredItemType);
        int costPerTrade = shop.GetRequiredItemQty(itemType);

        if (quantityText != null)
        {
            int finalQty = shop.IsSelling()
            ? currentQty - pendingQty
            : currentQty + pendingQty;

                    quantityText.text = "(" + finalQty.ToString() + ")";
        }
 

        if (pendingText != null)
            pendingText.text = "(" + pendingQty.ToString() + ")";

        if (goldValueText != null && shop.TryGetGoldValue(itemType, out int goldValue))
            goldValueText.text = goldValue.ToString();

        if (requiredItemQtyText != null)
        {
            if (costPerTrade > 0)
                requiredItemQtyText.text = costPerTrade.ToString();
            else
                requiredItemQtyText.text = "";
        }
        if (itemQtyText != null)
        {
            int remaining = currentRequiredItems - requiredPendingQty;
            itemQtyText.text = "(" + remaining.ToString() + ")";
        }
    }
}