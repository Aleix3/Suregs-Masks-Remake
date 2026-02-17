using UnityEngine;
using UnityEngine.Events;

public class ShopButton : MonoBehaviour
{
    public UnityEvent onSelect;
    public GameObject hoverVisual;

    public void Select()
    {
        if (onSelect != null)
            onSelect.Invoke();
    }

    public void SetHover(bool value)
    {
        if (hoverVisual != null)
            hoverVisual.SetActive(value);
    }
}
