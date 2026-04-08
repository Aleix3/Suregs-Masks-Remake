using TMPro;
using UnityEngine;


[System.Serializable]
public class BlackSmithTradeUI : MonoBehaviour
{
    public BlacksmithShop.BlacksmithMode mode;

    public BlacksmithShop blacksmithShop;

    public TextMeshProUGUI goldValueText;
    public TextMeshProUGUI requiredItemPendingText;
    public TextMeshProUGUI requiredItemQtyText;

    private void Start()
    {
        //shop = shopBehaviour as IShop;

        //if (shop == null)
        //{
        //    Debug.LogError("El objeto asignado no implementa IShop");
        //    return;
        //}

        //shop.OnTradeUpdated += Refresh;

        if (InventoryManager.instance != null)
            InventoryManager.instance.OnInventoryChanged -= Refresh;

        if (PlayerEconomy.instance != null)
            PlayerEconomy.instance.OnGoldChanged -= Refresh;

        Refresh();
    }

    private void OnEnable()
    {
        //shop = shopBehaviour as IShop;

        //if (shop == null)
        //{
        //    Debug.LogError("El objeto no implementa IShop");
        //    return;
        //}

        //shop.OnTradeUpdated += Refresh;

        //Refresh();
    }

    private void OnDisable()
    {
        //if (shop != null)
        //    shop.OnTradeUpdated -= Refresh;

        if (InventoryManager.instance != null)
            InventoryManager.instance.OnInventoryChanged -= Refresh;

        if (PlayerEconomy.instance != null)
            PlayerEconomy.instance.OnGoldChanged -= Refresh;
    }

    public void Refresh()
    {
        if (blacksmithShop == null) return;

        // obtenemos el trade dinámico
        BlackSmithTrade trade = blacksmithShop.GetTradeByMode(mode);

        if (trade == null)
        {
            goldValueText.text = "-";
            requiredItemQtyText.text = "";
            requiredItemPendingText.text = "";
            return;
        }

        int pendingQty = blacksmithShop.GetPending(trade.requiredItem);

        if (requiredItemPendingText != null)
            requiredItemPendingText.text = "(" + pendingQty.ToString() + ")";

        if (goldValueText != null)
            goldValueText.text = trade.goldCost.ToString();

        if (requiredItemQtyText != null)
        {
            if (trade.requiredItemQty > 0)
                requiredItemQtyText.text = trade.requiredItemQty.ToString();
            else
                requiredItemQtyText.text = "";
        }
    }
}
