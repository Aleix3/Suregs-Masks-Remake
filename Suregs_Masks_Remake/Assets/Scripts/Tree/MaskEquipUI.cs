using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Panel de equipar máscara.
/// Al pulsar E en "Primary Mask" o "Secondary Mask", aparece este panel
/// sobre el grid mostrando las máscaras desbloqueadas para elegir.
/// 
/// JERARQUÍA:
/// EquipPanel (este script)
/// ├── MaskPickButton_0  (Button + MaskPickButton)
/// ├── MaskPickButton_1
/// ├── MaskPickButton_2
/// └── MaskPickButton_3
/// </summary>
public class MaskEquipUI : MonoBehaviour
{
    [Header("Panel negro que tapa el grid")]
    public GameObject panel;

    [Header("Botones de selección (4, uno por máscara)")]
    public Button[]    pickButtons      = new Button[4];
    public Image[]     pickIcons        = new Image[4];
    public TextMeshProUGUI[] pickNames  = new TextMeshProUGUI[4];

    [Header("Colores")]
    public Color colorAvailable = Color.white;
    public Color colorLocked    = new Color(0.3f, 0.3f, 0.3f, 1f);

    // ── estado ────────────────────────────────────────────────────
    private bool         _equipingPrimary;
    private MaskTreeManager _tm;
    private MaskManager  _mm;
    private MaskTreeUI   _treeUI;

    [Header("Primer botón seleccionado al abrir el picker")]
    public GameObject firstPickButton;

    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        _tm     = MaskTreeManager.Instance;
        _mm     = Player.Instance?.MaskManager;
        _treeUI = FindAnyObjectByType<MaskTreeUI>();

        for (int m = 0; m < 4; m++)
        {
            int idx = m;
            pickButtons[m]?.onClick.AddListener(() => PickMask(idx));
        }

        panel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    //  API pública — llamada desde los botones de equipar
    // ─────────────────────────────────────────────────────────────
    public void OpenForPrimary()   => Open(primary: true);
    public void OpenForSecondary() => Open(primary: false);

    private void Open(bool primary)
    {
        _equipingPrimary = primary;
        panel.SetActive(true);
        RefreshPickButtons();

        if (firstPickButton != null)
            EventSystem.current.SetSelectedGameObject(firstPickButton);
    }

    public void Close()
    {
        panel.SetActive(false);
        // Devolver foco al botón correcto del árbol
        _treeUI?.RestoreFocus();
    }

    // ─────────────────────────────────────────────────────────────
    //  Elegir máscara
    // ─────────────────────────────────────────────────────────────
    private void PickMask(int maskIndex)
    {
        if (_mm == null) return;

        bool unlocked = _tm.masks[maskIndex]?.data?.isUnlocked ?? false;
        if (!unlocked) return;

        BaseMask chosen = _tm.masks[maskIndex];

        if (_equipingPrimary)
        {
            // Si la elegida ya era la secundaria, hacer swap
            BaseMask secondary = chosen == _mm.Secondary ? _mm.Primary : _mm.Secondary;
            _mm.SetPrimary(chosen, secondary);
        }
        else
        {
            // Si la elegida ya era la primaria, hacer swap
            BaseMask primary = chosen == _mm.Primary ? _mm.Secondary : _mm.Primary;
            _mm.SetPrimary(primary, chosen);
        }

        _treeUI?.RefreshEquippedButtons();
        Close();
    }

    // ─────────────────────────────────────────────────────────────
    //  Refresh
    // ─────────────────────────────────────────────────────────────
    private void RefreshPickButtons()
    {
        for (int m = 0; m < 4; m++)
        {
            bool unlocked = _tm.masks[m]?.data?.isUnlocked ?? false;

            if (pickButtons[m] != null)
                pickButtons[m].interactable = unlocked;

            if (pickIcons[m] != null)
            {
                pickIcons[m].enabled = unlocked && _tm.masks[m]?.data?.maskIcon != null;
                if (pickIcons[m].enabled)
                    pickIcons[m].sprite = _tm.masks[m].data.maskIcon;
                pickIcons[m].color = unlocked ? colorAvailable : colorLocked;
            }

            if (pickNames[m] != null)
                pickNames[m].text = unlocked ? (_tm.masks[m]?.data?.maskName ?? "") : "???";
        }
    }
}
