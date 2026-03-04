using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public List<ShopButton> buttons = new List<ShopButton>();

    public int currentIndex = 0;
    public int columns = 2;
    public bool isOpen = false;

    [Header("Scroll")]
    public ScrollRect scrollRect;
    public float scrollMargin = 10f;

    public RectTransform knobRect;
    public float topY = 245f;
    public float bottomY = -245f;

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

        if (Input.GetKeyDown(KeyCode.D))
        {
            if ((currentIndex % columns) < columns - 1 &&
                currentIndex + 1 < buttons.Count)
            {
                currentIndex++;
                UpdateHover();
            }
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if ((currentIndex % columns) > 0)
            {
                currentIndex--;
                UpdateHover();
            }
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

        if (scrollRect != null && buttons.Count > 0)
        {
            RectTransform selectedRect = buttons[currentIndex].GetComponent<RectTransform>();

            // Solo hacer scroll si el botón está dentro del content
            if (selectedRect.IsChildOf(scrollRect.content))
            {
                EnsureVisible(selectedRect);
                UpdateKnobManual(selectedRect);
            }
        }
    }

    // Hace visible el elemento dentro del viewport moviendo el content si hace falta
    void EnsureVisible(RectTransform item)
    {
        if (item == null || scrollRect == null || scrollRect.content == null) return;

        Canvas.ForceUpdateCanvases();

        RectTransform viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();

        Bounds viewportBounds = new Bounds(viewport.rect.center, viewport.rect.size);
        Bounds itemBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, item);

        Vector2 contentAnchored = scrollRect.content.anchoredPosition;

        if (itemBounds.min.y < viewportBounds.min.y + scrollMargin)
        {
            float diff = (viewportBounds.min.y + scrollMargin) - itemBounds.min.y;
            contentAnchored.y += diff;
            scrollRect.content.anchoredPosition = contentAnchored;
        }
        else if (itemBounds.max.y > viewportBounds.max.y - scrollMargin)
        {
            float diff = itemBounds.max.y - (viewportBounds.max.y - scrollMargin);
            contentAnchored.y -= diff;
            scrollRect.content.anchoredPosition = contentAnchored;
        }
    }

    // Calcula manualmente la posición del knob basada en el botón seleccionado
    void UpdateKnobManual(RectTransform selectedButton)
    {
        if (scrollRect == null || knobRect == null) return;

        // Solo si está dentro del scroll
        if (!selectedButton.IsChildOf(scrollRect.content)) return;

        // Contar cuántos botones hay dentro del ScrollRect
        List<ShopButton> scrollButtons = new List<ShopButton>();

        foreach (var b in buttons)
        {
            if (b.GetComponent<RectTransform>().IsChildOf(scrollRect.content))
                scrollButtons.Add(b);
        }

        if (scrollButtons.Count <= 1)
        {
            knobRect.anchoredPosition = new Vector2(knobRect.anchoredPosition.x, topY);
            return;
        }

        // Índice del botón actual dentro del scroll
        int scrollIndex = scrollButtons.IndexOf(buttons[currentIndex]);

        // 0 → arriba | último → abajo
        float normalized = (float)scrollIndex / (scrollButtons.Count - 1);

        float y = Mathf.Lerp(topY, bottomY, normalized);

        knobRect.anchoredPosition = new Vector2(knobRect.anchoredPosition.x, y);
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