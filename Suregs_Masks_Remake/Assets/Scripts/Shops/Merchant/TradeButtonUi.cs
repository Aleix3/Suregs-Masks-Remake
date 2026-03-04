using UnityEngine;
using TMPro;
using static Item;

public class TradeButtonUI : MonoBehaviour
{
    public Merchant merchant;
    public ItemType itemType;

    public TextMeshProUGUI quantityText;
    public TextMeshProUGUI pendingText;
    public TextMeshProUGUI goldValueText;

    private void Start()
    {
        merchant.OnTradeUpdated += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        merchant.OnTradeUpdated -= Refresh;
    }

    void Refresh()
    {
        int currentQty = InventoryManager.instance.GetQuantity(itemType);
        int pendingQty = merchant.GetPending(itemType);

        quantityText.text = "(" + currentQty.ToString() + ")";
        pendingText.text = pendingQty.ToString();

        if (merchant.TryGetGoldValue(itemType, out int goldValue))
        {
            goldValueText.text = goldValue.ToString();
        }
    }
}