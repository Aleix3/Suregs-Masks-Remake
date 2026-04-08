using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class Item : MonoBehaviour
{
    public enum ItemType {
        ITEM,
        GARRA,
        OJO,
        DIAMANTE,
        VISCERA,
        RUBI,
        POLVORA,
        NOTA,
        HUESO,
        DIENTE,
        COLA,
        CARBON,
        AMATISTA,
        ESPADA2,
        ARMADURA,
        PEZ_PEQUENO,
        PEZ_MEDIANO,
        PEZ_GRANDE,
        SALIVA,
        POCION_VIDA_1,
        POCION_VIDA_2,
        POCION_VIDA_3,
        POCION_VIDA_MAX,
        POCION_REGENERACION,
        POCION_DANO,
        POCION_VELOCIDAD,
        ORBE_MAGICO,
        ESPADA_NV2,
        ESPADA_NV3,
        ESPADA_NV4,
        ESPADA_NV5,
        ESPADA_NV6,
        ESPADA_NV7,
        ESPADA_NV8,
        ESPADA_NV9,
        ESPADA_NV10,
        ARMADURA_NV2,
        ARMADURA_NV3,
        ARMADURA_NV4,
        ARMADURA_NV5,
        ARMADURA_NV6,
        ARMADURA_NV7,
        ARMADURA_NV8,
        ARMADURA_NV9,
        ARMADURA_NV10,
        ZAFIRO,
        BASURA,
    }
    public ItemType type;
    int id;
    int ObjectId = -1;
    public string itemName;
    public string itemType;
    public SpriteRenderer sr;
    uint quantity = 1;
    bool inList = false;
    
    public string description;
    // Start is called before the first frame update

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        GetItemData(type, out itemName, out description, out itemType, out Sprite sprite);

        if (sr != null)
            sr.sprite = sprite;
    }

    public static void GetItemData(ItemType type, out string name, out string description, out string itemType, out Sprite sprite)
    {
        name = "";
        description = "";
        itemType = "";
        sprite = null;

        switch (type)
        {
            case ItemType.RUBI:
                name = "Rubí";
                description = "Mineral robusto extremadamente resistente.";
                itemType = "Material";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_rubi");
                break;

            case ItemType.DIAMANTE:
                name = "Diamante";
                description = "Mineral con increíbles propiedades para el combate.";
                itemType = "Material";
                sprite = Resources.Load<Sprite>("Textures/Items/texture_diamond");
                break;

            case ItemType.POLVORA:
                name = "Pólvora";
                description = "Mezcla explosiva muy versátil.";
                itemType = "Material";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_polvora");
                break;

            case ItemType.AMATISTA:
                name = "Amatista";
                description = "Piedra preciosa con gran valor.";
                itemType = "Material";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_amatista");
                break;

            case ItemType.CARBON:
                name = "Carbón";
                description = "Mineral con valor medio en el mercado.";
                itemType = "Material";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_carbon");
                break;

            case ItemType.SALIVA:
                name = "Saliva";
                description = "Líquido biológico usado en las pócimas.";
                itemType = "Apéndice";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_saliva");
                break;

            case ItemType.DIENTE:
                name = "Colmillo";
                description = "Colmillo con una resistencia sorprendente.";
                itemType = "Apéndice";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_diente");
                break;

            case ItemType.GARRA:
                name = "Garra";
                description = "Afilada garra capacitada para rajar piedras.";
                itemType = "Apéndice";
                sprite = Resources.Load<Sprite>("Textures/Items/item_Garra");
                break;

            case ItemType.OJO:
                name = "Ojo";
                description = "Globo ocular bastante asqueroso.";
                itemType = "Apéndice";
                sprite = Resources.Load<Sprite>("Textures/Items/texture_eye");
                break;

            case ItemType.HUESO:
                name = "Hueso";
                description = "Hueso de osiris, bastante resistente.";
                itemType = "Apéndice";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_hueso");
                break;

            case ItemType.VISCERA:
                name = "Vísceras";
                description = "Vísceras con propiedades en la alquimia.";
                itemType = "Apéndice";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_visceras");
                break;

            case ItemType.COLA:
                name = "Cola";
                description = "Cola sin propiedades especiales, puede valer dinero.";
                itemType = "Apéndice";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_cola");
                break;

            case ItemType.POCION_VIDA_1:
                name = "Pócima";
                description = "Curiosa pócima que revitaliza la salud levemente.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_1");
                break;

            case ItemType.POCION_VIDA_2:
                name = "Pócima";
                description = "Curiosa pócima que revitaliza buena parte de la salud.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_2");
                break;

            case ItemType.POCION_VIDA_3:
                name = "Pócima";
                description = "Curiosa pócima que revitaliza gran parte de la salud.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_3");
                break;

            case ItemType.POCION_VIDA_MAX:
                name = "Pócima";
                description = "Curiosa pócima que revitaliza la salud por completo.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_max");
                break;

            case ItemType.POCION_REGENERACION:
                name = "Pócima";
                description = "Curiosa pócima que aumenta tu regeneración de manera momentánea.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_regeneracion");
                break;

            case ItemType.POCION_DANO:
                name = "Pócima";
                description = "Curiosa pócima que aumenta tu fuerza de manera momentánea.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_dano");
                break;

            case ItemType.POCION_VELOCIDAD:
                name = "Pócima";
                description = "Curiosa pócima que aumenta tu velocidad de manera momentánea.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_velocidad");
                break;

            case ItemType.ORBE_MAGICO:
                name = "Orbe Mágico";
                description = "Objeto mágico con propiedades sorprendentes.";
                itemType = "Consumible";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_orbe_magico");
                break;

            case ItemType.ZAFIRO:
                name = "Zafiro";
                description = "Mineral afilado con propiedades para el combate.";
                itemType = "Material";
                sprite = Resources.Load<Sprite>("Textures/Items/textura_zafiro");
                break;

            case ItemType.BASURA:
                name = "Basura";
                description = "No parece que sirva para nada.";
                itemType = "Pez";
                sprite = null; // en XML ponía "---"
                break;
        }

        if (sprite == null)
        {
            string spriteName = GetSpriteName(type);

            if (!string.IsNullOrEmpty(spriteName))
            {
                sprite = Resources.Load<Sprite>("Textures/Items/" + spriteName);
            }
        }
    }

    private static string GetSpriteName(ItemType type)
    {
        switch (type)
        {
            case ItemType.RUBI: return "textura_rubi";
            case ItemType.DIAMANTE: return "texture_diamond";
            case ItemType.POLVORA: return "textura_polvora";
            case ItemType.AMATISTA: return "textura_amatista";
            case ItemType.CARBON: return "textura_carbon";
            case ItemType.SALIVA: return "textura_saliva";
            case ItemType.DIENTE: return "textura_diente";
            case ItemType.GARRA: return "item_Garra";
            case ItemType.OJO: return "texture_eye";
            case ItemType.HUESO: return "textura_hueso";
            case ItemType.VISCERA: return "textura_visceras";
            case ItemType.COLA: return "textura_cola";

            case ItemType.POCION_VIDA_1: return "textura_pocion_vida_1";
            case ItemType.POCION_VIDA_2: return "textura_pocion_vida_2";
            case ItemType.POCION_VIDA_3: return "textura_pocion_vida_3";
            case ItemType.POCION_VIDA_MAX: return "textura_pocion_vida_max";
            case ItemType.POCION_REGENERACION: return "textura_pocion_regeneracion";
            case ItemType.POCION_DANO: return "textura_pocion_dano";
            case ItemType.POCION_VELOCIDAD: return "textura_pocion_velocidad";

            case ItemType.ORBE_MAGICO: return "textura_orbe_magico";

            case ItemType.ZAFIRO: return "textura_zafiro";

            case ItemType.BASURA: return null;

            default:
                if (type.ToString().StartsWith("ESPADA"))
                    return type.ToString().ToLower();

                if (type.ToString().StartsWith("ARMADURA"))
                    return type.ToString().ToLower();

                return null;
        }
    }

}
