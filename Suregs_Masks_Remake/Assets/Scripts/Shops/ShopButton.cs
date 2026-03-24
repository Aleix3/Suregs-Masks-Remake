using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Item;

public class ShopButton : MonoBehaviour
{
    public GameObject hoverVisual;
    Button button;
    public ItemType itemType;

    private void Start()
    {
        button = GetComponent<Button>();
        if(this.gameObject.GetComponent<TradeButtonUI>() != null)
            itemType = this.gameObject.GetComponent<TradeButtonUI>().itemType;
    }

    public void Select()
    {
        button.onClick.Invoke();
    }

    public void SetHover(bool value)
    {
        if (hoverVisual != null)
            hoverVisual.SetActive(value);
    }
}
