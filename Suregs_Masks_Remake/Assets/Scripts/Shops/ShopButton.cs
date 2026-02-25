using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ShopButton : MonoBehaviour
{
    public GameObject hoverVisual;
    Button button;

    private void Start()
    {
        button = GetComponent<Button>();
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
