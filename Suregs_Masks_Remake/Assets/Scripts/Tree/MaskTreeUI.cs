using UnityEngine;
using UnityEngine.UI;
using TMPro;



/// JERARQUÍA ESPERADA EN EL INSPECTOR:
///
/// SkillTreePanel
/// ├── MaskSelector (4 botones — uno por máscara)
/// │   ├── MaskBtn_0  (Image + Button)
/// │   ├── MaskBtn_1
/// │   ├── MaskBtn_2
/// │   └── MaskBtn_3
/// ├── Grid (4 filas × 4 columnas de botones de mejora)
/// │   └── UpgradeBtn_[0-15]  (Image + Button)
/// ├── InfoPanel
/// │   ├── UpgradeName  (TMP)
/// │   └── UpgradeDesc  (TMP)
/// ├── PrimaryMaskIcon   (Image)
/// ├── SecondaryMaskIcon (Image)
/// ├── PointsText        (TMP)  "X/8 Mask Points"
/// └── UpgradesText      (TMP)  "X/8 Mejoras"

public class MaskTreeUI : MonoBehaviour
{
    public Button[] maskButtons       = new Button[4];
    public Image[]  maskButtonIcons   = new Image[4];

    public Button[] upgradeButtons    = new Button[16];
    public Image[]  upgradeIcons      = new Image[16];

    public TextMeshProUGUI upgradeName;
    public TextMeshProUGUI upgradeDesc;
    public Image           upgradeIcon;

    public Image primaryMaskIcon;
    public Image secondaryMaskIcon;

    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI upgradesText;

    [Header("Colores de estado")]
    public Color colorUnlocked  = new Color(1f,  1f,  1f,  1f);
    public Color colorLocked    = new Color(0.3f,0.3f,0.3f,1f);
    public Color colorSelected  = new Color(0.8f,0.6f,0f,  1f);

    [Header("Datos de mejoras (16 entradas: rama0lv1..lv4, rama1lv1..lv4, ...)")]
    public UpgradeNodeData[] nodeData = new UpgradeNodeData[16];

    [System.Serializable]
    public struct UpgradeNodeData
    {
        public string      upgradeName;
        [TextArea] public string upgradeDesc;
        public Sprite      icon;
    }

    private int  _selectedMask   = 0;
    private int  _hoveredNode    = -1;
    private MaskTreeManager _tm;

    private void Start()
    {
        _tm = MaskTreeManager.Instance;

        for (int m = 0; m < 4; m++)
        {
            int idx = m;
            maskButtons[m]?.onClick.AddListener(() => SelectMask(idx));
        }

        for (int n = 0; n < 16; n++)
        {
            int nodeIdx = n;
            upgradeButtons[n]?.onClick.AddListener(() => OnUpgradeClicked(nodeIdx));
        }

        _tm.OnTreeChanged += OnTreeChanged;

        SelectMask(0);
    }

    private void OnDestroy()
    {
        if (_tm != null) _tm.OnTreeChanged -= OnTreeChanged;
    }

    private void OnEnable() => Refresh();

    private void SelectMask(int maskIndex)
    {
        _selectedMask = maskIndex;
        Refresh();
    }

    private void OnUpgradeClicked(int nodeIndex)
    {
        int branch = nodeIndex / 4;
        int level  = nodeIndex % 4;

        // Solo se puede comprar el siguiente nivel de la rama
        int currentLevel = _tm.GetLevel(_selectedMask, branch);
        if (level != currentLevel)
        {
            Debug.Log("[SkillTreeUI] Debes comprar los niveles anteriores primero.");
            return;
        }

        _tm.TryUpgrade(_selectedMask, branch);
    }
    private void OnTreeChanged(int maskIndex)
    {
        if (maskIndex == _selectedMask) Refresh();
    }

    private void Refresh()
    {
        if (_tm == null) return;

        RefreshMaskButtons();
        RefreshGrid();
        RefreshStats();
        RefreshEquippedMasks();
    }

    private void RefreshMaskButtons()
    {
        for (int m = 0; m < 4; m++)
        {
            if (maskButtonIcons[m] == null) continue;
            // Resaltar la seleccionada
            maskButtonIcons[m].color = m == _selectedMask ? colorSelected : colorUnlocked;

            BaseMask mask = _tm.masks[m];
            if (mask != null && mask.data?.maskIcon != null)
                maskButtonIcons[m].sprite = mask.data.maskIcon;
        }
    }

    private void RefreshGrid()
    {
        for (int branch = 0; branch < 4; branch++)
        {
            int branchLevel = _tm.GetLevel(_selectedMask, branch);

            for (int lvl = 0; lvl < 4; lvl++)
            {
                int nodeIndex = branch * 4 + lvl;
                if (nodeIndex >= upgradeButtons.Length) break;

                Button btn   = upgradeButtons[nodeIndex];
                Image  icon  = upgradeIcons[nodeIndex];
                if (btn == null) continue;

                bool isUnlocked  = lvl < branchLevel;     
                bool isNext      = lvl == branchLevel;    
                bool isAvailable = isNext && _tm.CanUpgrade(_selectedMask, branch);

                if (icon != null)
                {
                    if (isUnlocked)       icon.color = colorSelected;
                    else if (isAvailable) icon.color = colorUnlocked;
                    else                  icon.color = colorLocked;

                    if (nodeIndex < nodeData.Length && nodeData[nodeIndex].icon != null)
                        icon.sprite = nodeData[nodeIndex].icon;
                }

                btn.interactable = isAvailable;
            }
        }
    }

    private void RefreshStats()
    {
        if (pointsText  != null)
            pointsText.text  = $"{_tm.GetPoints(_selectedMask)}/8 Mask Points";
        if (upgradesText != null)
            upgradesText.text = $"{_tm.GetTotalUpgrades(_selectedMask)}/8 Mejoras";
    }

    private void RefreshEquippedMasks()
    {
        var mm = Player.Instance?.MaskManager;
        if (mm == null) return;

        SetMaskIcon(primaryMaskIcon,   mm.Primary);
        SetMaskIcon(secondaryMaskIcon, mm.Secondary);
    }

    private void SetMaskIcon(Image img, BaseMask mask)
    {
        if (img == null) return;
        if (mask?.data?.maskIcon != null)
        {
            img.sprite  = mask.data.maskIcon;
            img.enabled = true;
        }
        else
        {
            img.enabled = false;
        }
    }

    public void OnNodeHoverEnter(int nodeIndex)
    {
        _hoveredNode = nodeIndex;
        if (nodeIndex < 0 || nodeIndex >= nodeData.Length) return;

        if (upgradeName != null) upgradeName.text = nodeData[nodeIndex].upgradeName;
        if (upgradeDesc != null) upgradeDesc.text  = nodeData[nodeIndex].upgradeDesc;
        if (upgradeIcon != null && nodeData[nodeIndex].icon != null)
            upgradeIcon.sprite = nodeData[nodeIndex].icon;
    }

    public void OnNodeHoverExit()
    {
        _hoveredNode = -1;
    }
}
