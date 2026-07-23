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
    public bool isPrimary;
    public Image maskIcon;

    [HideInInspector] public GameObject hover;

    public MaskEquipUI _equipUI;
    private MaskManager _mm;

    private void Awake()
    {
        hover = transform.Find("Hover")?.gameObject;
        _mm = Player.Instance?.MaskManager;

        if (_mm != null) _mm.OnSwap -= OnSwapChanged;
        if (_mm != null) _mm.OnSwap += OnSwapChanged;
    }

    private void OnEnable()
    {
        

        RefreshIcon();
    }

    private void OnSwapChanged(BaseMask p, BaseMask s) => RefreshIcon();

    private void RefreshIcon()
    {
        if (maskIcon == null || _mm == null) return;
        BaseMask mask = isPrimary ? _mm.Primary : _mm.Secondary;

        if (mask?.data?.maskIcon != null)
        {
            maskIcon.sprite = mask.data.maskIcon;
            maskIcon.preserveAspect = true;
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
        AudioManager.Instance.PlaySFX(AudioManager.Instance.selectClip);
        hover?.SetActive(true);
    }

    public void OnDeselect(BaseEventData e)
    {
        hover?.SetActive(false);
    }

    // E → abrir picker
    public void OnSubmit(BaseEventData e)
    {
        AudioManager.Instance.PlaySFX(AudioManager.Instance.buttonClip);
        if (isPrimary) _equipUI.OpenForPrimary();
        else _equipUI.OpenForSecondary();
    }
}