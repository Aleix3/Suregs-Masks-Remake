using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Añadir a los botones "Primary Mask" y "Secondary Mask".
/// Muestra la máscara equipada en su icono y abre el picker al pulsar E.
/// </summary>
public class EquipButton : MonoBehaviour,
    ISelectHandler, IDeselectHandler, ISubmitHandler
{
    public bool         isPrimary;
    public Image        maskIcon;

    [HideInInspector] public GameObject hover;

    private MaskEquipUI  _equipUI;
    private MaskManager  _mm;

    private void Awake()
    {
        _equipUI = FindAnyObjectByType<MaskEquipUI>();
        _mm      = Player.Instance?.MaskManager;
        hover    = transform.Find("Hover")?.gameObject;

        // Escuchar cambios de máscara equipada
        if (_mm != null) _mm.OnSwap += OnSwapChanged;
    }

    private void OnDestroy()
    {
        if (_mm != null) _mm.OnSwap -= OnSwapChanged;
    }

    private void OnEnable() => RefreshIcon();

    private void OnSwapChanged(BaseMask p, BaseMask s) => RefreshIcon();

    private void RefreshIcon()
    {
        if (maskIcon == null || _mm == null) return;
        BaseMask mask = isPrimary ? _mm.Primary : _mm.Secondary;

        if (mask?.data?.maskIcon != null)
        {
            maskIcon.sprite  = mask.data.maskIcon;
            maskIcon.enabled = true;
            // restaurar alpha por si estaba oculto
            var c = maskIcon.color; c.a = 1f; maskIcon.color = c;
        }
        else
        {
            maskIcon.enabled = false;
        }
    }

    // ── Navegación WASD ───────────────────────────────────────────
    public void OnSelect(BaseEventData e)
    {
        hover?.SetActive(true);
    }

    public void OnDeselect(BaseEventData e)
    {
        hover?.SetActive(false);
    }

    // E → abrir picker
    public void OnSubmit(BaseEventData e)
    {
        if (isPrimary) _equipUI.OpenForPrimary();
        else           _equipUI.OpenForSecondary();
    }
}
