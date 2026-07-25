using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class JourneyManager : MonoBehaviour
{
    private CanvasGroup canvasGroup;

    [Header("SwordSlot")]
    public TextMeshProUGUI swordLevelText;
    public TextMeshProUGUI swordDescText;
    public Image swordIcon;

    [Header("ArmorSlot")]
    public TextMeshProUGUI armorLevelText;
    public TextMeshProUGUI armorDescText;
    public Image armorIcon;

    [Header("Masks — slot primaria")]
    public Image primaryMaskIcon;
    public TextMeshProUGUI primaryMaskName;
    public TextMeshProUGUI primaryMaskAbility;

    [Header("Masks — slot secundaria")]
    public Image secondaryMaskIcon;
    public TextMeshProUGUI secondaryMaskName;
    public TextMeshProUGUI secondaryMaskAbility;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }


    private void OnMaskChanged(BaseMask p, BaseMask s)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        // ── Espada ────────────────────────────────────────────────
        swordLevelText.text = "Nivel: " + Player.Instance.weaponLevel;
        swordDescText.text = "Ataque: " + PlayerStatsTable.GetWeaponDamage(Player.Instance.weaponLevel);

        // ── Armadura ──────────────────────────────────────────────
        armorLevelText.text = "Nivel: " + Player.Instance.armorLevel;
        armorDescText.text = "Vida: " + PlayerStatsTable.GetArmorHealth(Player.Instance.armorLevel);

        // ── Iconos de equipo ──────────────────────────────────────
        ItemType swordType = GetWeaponTypeByLevel(Player.Instance.weaponLevel);
        ItemType armorType = GetArmorTypeByLevel(Player.Instance.armorLevel);

        Item.GetItemData(swordType, out _, out _, out _, out Sprite swordSprite);
        Item.GetItemData(armorType, out _, out _, out _, out Sprite armorSprite);

        SetEquipIcon(swordIcon, swordSprite);
        SetEquipIcon(armorIcon, armorSprite);

        // ── Máscaras ──────────────────────────────────────────────
        MaskManager mm = MaskManager.Instance;

        if (mm != null && (mm.Primary != null || mm.Secondary != null))
        {
            if(mm.Primary != null)
            {
                SetMaskSlot(mm.Primary, primaryMaskIcon, primaryMaskName, primaryMaskAbility,
                        mm.Primary.data.abilityDescription);
            }

            if (mm.Secondary != null)
            {
                SetMaskSlot(mm.Secondary, secondaryMaskIcon, secondaryMaskName, secondaryMaskAbility,
                        mm.Secondary.data.passiveDescription);
            }

                
        }
        else
        {
            ClearMaskSlot(primaryMaskIcon, primaryMaskName, primaryMaskAbility);
            ClearMaskSlot(secondaryMaskIcon, secondaryMaskName, secondaryMaskAbility);
        }
    }


    private void SetMaskSlot(BaseMask mask, Image icon, TextMeshProUGUI nameText, TextMeshProUGUI abilityText, string description)
    {
        if (mask == null || mask.data == null)
        {
            ClearMaskSlot(icon, nameText, abilityText);
            return;
        }

        // Icono
        if (icon != null)
        {
            icon.sprite = mask.data.maskIcon;
            icon.enabled = mask.data.maskIcon != null;
        }


        if (nameText != null) nameText.text = mask.data.maskName;


        if (abilityText != null) abilityText.text = description;
    }

    private void ClearMaskSlot(Image icon, TextMeshProUGUI nameText, TextMeshProUGUI abilityText)
    {
        if (icon != null) { icon.sprite = null; icon.enabled = false; }
        if (nameText != null) nameText.text = "—";
        if (abilityText != null) abilityText.text = "—";
    }

    private void SetEquipIcon(Image icon, Sprite sprite)
    {
        if (icon == null) return;
        if (sprite != null)
        {
            icon.sprite = sprite;
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 1f);
        }
        else
        {
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0f);
        }
    }

    ItemType GetWeaponTypeByLevel(int level)
        => (ItemType)System.Enum.Parse(typeof(ItemType), $"ESPADA_NV{level}");

    ItemType GetArmorTypeByLevel(int level)
        => (ItemType)System.Enum.Parse(typeof(ItemType), $"ARMADURA_NV{level}");
}