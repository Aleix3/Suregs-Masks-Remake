using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Cambia el color del texto (TextMeshProUGUI) de un botón cuando está
/// resaltado: seleccionado por teclado/mando (OnSelect) o con el ratón encima
/// (OnPointerEnter). No usa el sistema de Color Tint nativo de Selectable
/// porque con TextMeshProUGUI a veces no refresca el color visualmente.
///
/// Ponlo en el mismo GameObject que el Button.
/// </summary>
public class ButtonTextHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightedColor = Color.red;

    private bool isPointerOver;
    private bool isSelected;

    private void OnEnable()
    {
        isPointerOver = false;
        isSelected = false;
        UpdateColor();
    }

    public void OnSelect(BaseEventData eventData) { isSelected = true; UpdateColor(); }
    public void OnDeselect(BaseEventData eventData) { isSelected = false; UpdateColor(); }
    public void OnPointerEnter(PointerEventData eventData) { isPointerOver = true; UpdateColor(); }
    public void OnPointerExit(PointerEventData eventData) { isPointerOver = false; UpdateColor(); }

    private void UpdateColor()
    {
        if (label == null) return;
        label.color = (isSelected || isPointerOver) ? highlightedColor : normalColor;
    }
}
