using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using static Item;

public class JourneyManager : MonoBehaviour
{
    [Header("SwordSlot")]
    public TextMeshProUGUI swordLevelText;
    public TextMeshProUGUI swordDescText;
    public Image swordIcon;

    [Header("ArmorSlot")]
    public TextMeshProUGUI armorLevelText;
    public TextMeshProUGUI armorDescText;
    public Image armorIcon;

    private void OnEnable()
    {
        UpdateUI();
    }
    public void UpdateUI()
    {
        swordLevelText.text = "Nivel: " + Player.Instance.weaponLevel;
        swordDescText.text = "Ataque: " + PlayerStatsTable.GetWeaponDamage(Player.Instance.weaponLevel);
        

        armorLevelText.text = "Nivel: " + Player.Instance.armorLevel;
        armorDescText.text = "Vida: " + PlayerStatsTable.GetWeaponDamage(Player.Instance.armorLevel);

        ItemType swordType = GetWeaponTypeByLevel(Player.Instance.weaponLevel);
        ItemType armorType = GetArmorTypeByLevel(Player.Instance.armorLevel);

        //obtener sprites desde el sistema
        Item.GetItemData(swordType, out _, out _, out _, out Sprite swordSprite);
        Item.GetItemData(armorType, out _, out _, out _, out Sprite armorSprite);
        if (swordSprite != null)
        {
            swordIcon.color = new Color(swordIcon.color.r, swordIcon.color.g, swordIcon.color.b, 100f);
            swordIcon.sprite = swordSprite;
        }
        else
        {
            swordIcon.color = new Color(swordIcon.color.r, swordIcon.color.g, swordIcon.color.b, 0f);
        }

        if (armorSprite != null)
        {
            armorIcon.color = new Color(armorIcon.color.r, armorIcon.color.g, armorIcon.color.b, 100f);
            armorIcon.sprite = armorSprite;
        }
        else
        {
            armorIcon.color = new Color(armorIcon.color.r, armorIcon.color.g, armorIcon.color.b, 0f);
        }
    }

    ItemType GetWeaponTypeByLevel(int level)
    {
        return (ItemType)System.Enum.Parse(typeof(ItemType), $"ESPADA_NV{level}");
    }

    ItemType GetArmorTypeByLevel(int level)
    {
        return (ItemType)System.Enum.Parse(typeof(ItemType), $"ARMADURA_NV{level}");
    }
}
