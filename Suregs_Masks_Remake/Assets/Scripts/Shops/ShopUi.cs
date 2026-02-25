using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public List<ShopButton> buttons = new List<ShopButton>();

    public int currentIndex = 0;

    public bool isOpen = false;

    [Header("Scroll")]
    public ScrollRect scrollRect;               // Arrastra aquí tu ScrollRect desde el Inspector
    public float scrollMargin = 10f;            // margen en píxeles para que no quede pegado al borde

    void Start()
    {
        UpdateHover();
    }

    void Update()
    {
        if (!isOpen) return;

        HandleInput();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            currentIndex++;
            if (currentIndex >= buttons.Count)
                currentIndex = 0;

            UpdateHover();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = buttons.Count - 1;

            UpdateHover();
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            buttons[currentIndex].Select();
        }
    }

    void UpdateHover()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].SetHover(i == currentIndex);
        }

        // Aseguramos que el botón seleccionado quede visible en la ScrollRect
        if (scrollRect != null && buttons.Count > 0)
        {
            RectTransform selectedRect = buttons[currentIndex].GetComponent<RectTransform>();
            EnsureVisible(selectedRect);
        }
    }

    // Hace visible el elemento dentro del viewport moviendo el content si hace falta
    void EnsureVisible(RectTransform item)
    {
        if (item == null || scrollRect == null || scrollRect.content == null) return;

        // forzamos actualización de layout (necesario si usas LayoutGroups)
        Canvas.ForceUpdateCanvases();

        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

        // bounds en espacio local del viewport
        Bounds viewportBounds = new Bounds(viewport.rect.center, viewport.rect.size);
        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, item);

        Vector2 contentAnchored = scrollRect.content.anchoredPosition;

        // Si la parte inferior del item está por debajo del viewport (no visible)
        if (itemBounds.min.y < viewportBounds.min.y + scrollMargin)
        {
            float diff = (viewportBounds.min.y + scrollMargin) - itemBounds.min.y;
            contentAnchored.y += diff;
            scrollRect.content.anchoredPosition = contentAnchored;
        }
        // Si la parte superior del item está por encima del viewport (no visible)
        else if (itemBounds.max.y > viewportBounds.max.y - scrollMargin)
        {
            float diff = itemBounds.max.y - (viewportBounds.max.y - scrollMargin);
            contentAnchored.y -= diff;
            scrollRect.content.anchoredPosition = contentAnchored;
        }
    }

    public void Open()
    {
        isOpen = true;
        currentIndex = 0;
        UpdateHover();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        isOpen = false;
        gameObject.SetActive(false);
    }
}