using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class BlackSmithTradeUI : MonoBehaviour
{
    public BlacksmithShop.BlacksmithMode mode;

    public BlacksmithShop blacksmithShop;

    public TextMeshProUGUI goldValueText;
    public TextMeshProUGUI requiredItemPendingText;
    public TextMeshProUGUI requiredItemQtyText;
    public TextMeshProUGUI levelText;

    public Image itemRequiredImage;
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
            {
                itemRequiredImage.gameObject.SetActive(true);
                Item.GetItemData(trade.requiredItem, out _, out _, out _, out Sprite requiredItemSprite);
                itemRequiredImage.sprite = requiredItemSprite;
                requiredItemQtyText.text = trade.requiredItemQty.ToString();
            }
            else
            {
                itemRequiredImage.gameObject.SetActive(false);
                requiredItemQtyText.text = "";
            }
                
        }

        if (mode == BlacksmithShop.BlacksmithMode.Weapon) 
        {
            levelText.text = "Nivel " + (Player.Instance.weaponLevel + 1);
        }
        else
        {
            levelText.text = "Nivel " + (Player.Instance.armorLevel + 1);
        }
    }
}
