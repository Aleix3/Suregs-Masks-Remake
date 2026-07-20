using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


[RequireComponent(typeof(Toggle))]
public class ToggleSpriteSwap : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [SerializeField] private Image targetGraphic;
    [SerializeField] private Sprite uncheckedSelectedSprite; // CheckBoxSelected
    [SerializeField] private Sprite checkedSprite;             // CheckBoxMarked
    [SerializeField] private Sprite checkedSelectedSprite;     // CheckBoxMarkedSelected

    private Toggle toggle;
    private bool isPointerOver;
    private bool isSelected;

    private bool IsHighlighted => isPointerOver || isSelected;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(OnValueChanged);
        
    }

    private void OnEnable() => UpdateVisual();
    private void OnDestroy() => toggle.onValueChanged.RemoveListener(OnValueChanged);
    private void OnValueChanged(bool _) => UpdateVisual();

    public void OnPointerEnter(PointerEventData eventData) 
    { 
        isPointerOver = true; UpdateVisual(); 
    }
    public void OnPointerExit(PointerEventData eventData) { isPointerOver = false; UpdateVisual(); }
    public void OnSelect(BaseEventData eventData) 
    { 
        isSelected = true; UpdateVisual(); 
    }
    public void OnDeselect(BaseEventData eventData) { isSelected = false; UpdateVisual(); }

    public void UpdateVisual()
    {
        if (targetGraphic == null) return;

        if (toggle.isOn)
        {
            SetSprite(IsHighlighted ? checkedSelectedSprite : checkedSprite);
        }
        else if (IsHighlighted)
        {
            SetSprite(uncheckedSelectedSprite);
        }
        else
        {
            // Sin marcar y sin resaltar: no hay sprite propio, se deja transparente
            // para que se vea la casilla ya dibujada en el fondo del panel.
            SetSprite(null);
        }
    }

    private void SetSprite(Sprite sprite)
    {
        targetGraphic.sprite = sprite;
        Color c = targetGraphic.color;
        c.a = sprite == null ? 0f : 1f;
        targetGraphic.color = c;
    }
}