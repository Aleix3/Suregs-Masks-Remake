using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MaskTreeUI : MonoBehaviour
{
    [Header("Botones de máscara (4)")]
    public Image[] maskButtonIcons = new Image[4];

    [Header("Botones de mejora (16)")]
    public Button[] upgradeButtons = new Button[16];
    public Image[] upgradeIcons = new Image[16];

    [Header("Panel de info")]
    public TextMeshProUGUI infoName;
    public TextMeshProUGUI infoDesc;
    public Image closeUpIcon;

    [Header("Máscaras equipadas")]
    public Image primaryMaskIcon;
    public Image secondaryMaskIcon;

    [Header("Stats")]
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI upgradesText;

    [Header("Colores")]
    public Color colorUnlocked = Color.white;
    public Color colorLocked = new Color(0.3f, 0.3f, 0.3f, 1f);
    public Color colorBought = new Color(0.8f, 0.6f, 0f, 1f);

    [Header("Datos (4 máscaras × 16 nodos)")]
    public MaskUpgradeSet[] maskData = new MaskUpgradeSet[4];

    [System.Serializable]
    public class MaskUpgradeSet
    {
        public string maskName;
        [TextArea] public string maskDesc;
        public UpgradeNodeData[] nodes = new UpgradeNodeData[16];
    }

    [System.Serializable]
    public struct UpgradeNodeData
    {
        public string upgradeName;
        [TextArea] public string upgradeDesc;
        public Sprite icon;
    }

    [Header("Primer botón seleccionado al abrir")]
    public GameObject firstButton;

    private int _activeMask = 0;
    private MaskTreeManager _tm;

    private void Awake()
    {

    }

    private void Start()
    {
        // Los clicks/submit los gestionan UpgradeNodeButton y MaskButton
        // directamente — no añadir listeners aquí para evitar doble llamada

        // Primer refresh aquí — MaskManager ya ha cargado desbloqueos en su Awake
        _tm = MaskTreeManager.Instance;
        _tm.OnTreeChanged += _ => RefreshGrid();
        InitialRefresh();
    }

    private void OnDestroy()
    {
        if (_tm != null) _tm.OnTreeChanged -= _ => RefreshGrid();
    }

    private void OnEnable()
    {
        // Asegurarse de que _tm está listo antes de hacer nada
        if (_tm == null) _tm = MaskTreeManager.Instance;
        if (_tm == null) return;

        if (firstButton != null)
            EventSystem.current.SetSelectedGameObject(firstButton);

        InitialRefresh();
    }

    private void InitialRefresh()
    {
        int firstUnlocked = -1;
        for (int m = 0; m < MaskTreeManager.MASK_COUNT; m++)
        {
            if (_tm.masks[m]?.data?.isUnlocked ?? false)
            {
                firstUnlocked = m;
                break;
            }
        }

        if (firstUnlocked >= 0)
        {
            firstButton?.GetComponent<MaskButton>()?.hover?.SetActive(true);
            ShowMask(firstUnlocked);
        }
        else
            ClearPanel();

        RefreshAll();
    }

    public void ClearPanel()
    {
        if (infoName != null) infoName.text = "";
        if (infoDesc != null) infoDesc.text = "";
        if (closeUpIcon != null) closeUpIcon.sprite = null;

        for (int i = 0; i < upgradeIcons.Length; i++)
            if (upgradeIcons[i] != null)
            {
                upgradeIcons[i].sprite = null;
                upgradeIcons[i].enabled = false;
            }

        if (pointsText != null) pointsText.text = "";
        if (upgradesText != null) upgradesText.text = "";
    }


    public void ShowMask(int maskIndex)
    {
        bool unlocked = _tm.masks[maskIndex]?.data?.isUnlocked ?? false;
        if (!unlocked) return;

        _activeMask = maskIndex;

        if (infoName != null) infoName.text = maskData[maskIndex].maskName;
        if (infoDesc != null) infoDesc.text = maskData[maskIndex].maskDesc;

        RefreshGrid();
        RefreshStats();
        RefreshMaskIcons();
    }

    public void BuyUpgrade(int nodeIndex)
    {
        int branch = nodeIndex / 4;
        int levelClicked = nodeIndex % 4;           // 0-3
        int currentLevel = _tm.GetLevel(_activeMask, branch);  // niveles comprados (0-4)

        bool isBought = levelClicked < currentLevel;
        bool isNext = levelClicked == currentLevel;
        bool isLast = levelClicked == currentLevel - 1;  // último comprado

        if (isBought && isLast)
        {
            // Deshacer el último nivel de la rama
            _tm.TryDowngrade(_activeMask, branch);
        }
        else if (isNext)
        {
            // Comprar el siguiente nivel
            _tm.TryUpgrade(_activeMask, branch);
        }
        else
        {
            Debug.Log("[SkillTree] Solo puedes deshacer el último nivel comprado o comprar el siguiente.");
        }
    }

    private void RefreshAll()
    {
        if (_tm == null) return;
        RefreshMaskIcons();
        RefreshGrid();
        RefreshStats();
        RefreshEquipped();
    }

    private void RefreshMaskIcons()
    {
        for (int m = 0; m < 4; m++)
        {
            if (maskButtonIcons[m] == null) continue;

            var mask = _tm.masks[m];
            bool unlocked = mask?.data?.isUnlocked ?? false;

            // Icono: visible solo si desbloqueada
            if (mask?.data?.maskIcon != null && unlocked)
            {
                maskButtonIcons[m].sprite = mask.data.maskIcon;
                maskButtonIcons[m].enabled = true;
            }
            else
            {
                maskButtonIcons[m].enabled = false;
            }

            maskButtonIcons[m].color = (m == _activeMask && unlocked)
                ? colorBought : Color.white;
        }

        // Sincronizar interactable de cada MaskButton
        foreach (var mb in FindObjectsByType<MaskButton>(FindObjectsSortMode.None))
            mb.RefreshInteractable();
    }

    private void RefreshGrid()
    {
        if (_tm == null) return;

        bool unlocked = _tm.masks[_activeMask]?.data?.isUnlocked ?? false;

        // Si la máscara activa no está desbloqueada, ocultar todo el grid
        if (!unlocked)
        {
            for (int i = 0; i < upgradeIcons.Length; i++)
                if (upgradeIcons[i] != null)
                {
                    upgradeIcons[i].sprite = null;
                    upgradeIcons[i].enabled = false;
                }
            return;
        }

        var nodes = maskData[_activeMask].nodes;

        for (int branch = 0; branch < 4; branch++)
        {
            int bought = _tm.GetLevel(_activeMask, branch);

            for (int lvl = 0; lvl < 4; lvl++)
            {
                int ni = branch * 4 + lvl;
                if (ni >= upgradeButtons.Length) break;

                Image icon = upgradeIcons[ni];
                if (icon != null)
                {
                    icon.enabled = true;
                    icon.color = lvl < bought ? colorBought :
                                   lvl == bought && _tm.CanUpgrade(_activeMask, branch)
                                                ? colorUnlocked : colorLocked;

                    if (ni < nodes.Length && nodes[ni].icon != null)
                        icon.sprite = nodes[ni].icon;
                }
            }
        }
    }

    private void RefreshStats()
    {
        if (pointsText != null)
            pointsText.text = $"{_tm.GetPoints(_activeMask)}/8 Mask Points";
        if (upgradesText != null)
            upgradesText.text = $"{_tm.GetTotalUpgrades(_activeMask)}/8 Mejoras";
    }

    private void RefreshEquipped()
    {
        var mm = Player.Instance?.MaskManager;
        if (mm == null) return;
        SetIcon(primaryMaskIcon, mm.Primary);
        SetIcon(secondaryMaskIcon, mm.Secondary);
    }

    private void SetIcon(Image img, BaseMask mask)
    {
        if (img == null) return;
        img.enabled = mask?.data?.maskIcon != null;
        if (img.enabled) img.sprite = mask.data.maskIcon;
    }

    public void OnNodeSelected(int nodeIndex)
    {
        bool unlocked = _tm.masks[_activeMask]?.data?.isUnlocked ?? false;
        if (!unlocked) return;

        var nodes = maskData[_activeMask].nodes;
        if (nodeIndex < 0 || nodeIndex >= nodes.Length) return;
        if (infoName != null) infoName.text = nodes[nodeIndex].upgradeName;
        if (infoDesc != null) infoDesc.text = nodes[nodeIndex].upgradeDesc;
        closeUpIcon.sprite = nodes[nodeIndex].icon;
        closeUpIcon.preserveAspect = true;
    }

    public void OnMaskSelected(int maskIndex)
    {
        bool unlocked = _tm?.masks[maskIndex]?.data?.isUnlocked ?? false;
        if (!unlocked) return;

        if (infoName != null) infoName.text = maskData[maskIndex].maskName;
        if (infoDesc != null) infoDesc.text = maskData[maskIndex].maskDesc;

        if (closeUpIcon != null && maskButtonIcons[maskIndex] != null
            && maskButtonIcons[maskIndex].sprite != null)
        {
            closeUpIcon.sprite = maskButtonIcons[maskIndex].sprite;
            closeUpIcon.preserveAspect = true;
            var c = closeUpIcon.color; c.a = 1f; closeUpIcon.color = c;
        }
    }

    public void RestoreFocus()
    {
        if (firstButton != null)
            EventSystem.current.SetSelectedGameObject(firstButton);
    }

    public void RefreshEquippedButtons() => RefreshEquipped();

    public void ClearHovers()
    {
        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            upgradeButtons[i].GetComponent<UpgradeNodeButton>().hover.SetActive(false);
        }
        for (int i = 0; i < maskButtonIcons.Length; i++)
        {
            maskButtonIcons[i].GetComponent<MaskButton>().hover.SetActive(false);
        }
    }
}