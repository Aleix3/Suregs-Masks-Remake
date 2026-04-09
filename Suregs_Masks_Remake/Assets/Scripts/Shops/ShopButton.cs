using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Item;

public class ShopButton : MonoBehaviour
{
    public GameObject hoverVisual;
    Button button;
    public ItemType itemType;
    public bool canBeSelected = true;
    [SerializeField] public bool isSelected = false;
    [SerializeField] public bool isSelectedPermanent = false;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private float blinkSpeed = 0.1f;
    [SerializeField] private int blinkCount = 3;

    private void Start()
    {
        button = GetComponent<Button>();
        if(this.gameObject.GetComponent<TradeButtonUI>() != null)
            itemType = this.gameObject.GetComponent<TradeButtonUI>().itemType;
    }

    public void Select()
    {
        
        if (canBeSelected)
        {
            isSelected = true;
            
        }
        button.onClick.Invoke();
    }
    public void DeSelect(bool wrong = false)
    {
        if (wrong)
        {
            isSelected = false;
            StartCoroutine(WrongAnimation());
        }
        else
        {
            if (canBeSelected)
            {
                isSelectedPermanent = false;
                isSelected = false;
                SetHover(false);
            }
        }
    }

    public void SelectPermanent()
    {
        isSelectedPermanent = true;
        SetHover(true);
    }
    public void SetHover(bool value)
    {
        if (hoverVisual != null)
            hoverVisual.SetActive(value);
    }

    private IEnumerator WrongAnimation()
    {
        Image img = hoverVisual.GetComponent<Image>();

        if (hoverVisual != null)
            hoverVisual.SetActive(true);

        Color originalColor = img.color;

        for (int i = 0; i < blinkCount; i++)
        {
            img.color = wrongColor;
            yield return new WaitForSeconds(blinkSpeed);

            img.color = originalColor;
            yield return new WaitForSeconds(blinkSpeed);
        }

        //SetHover(false);
    }
}
