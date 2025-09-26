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
        switch (type)
        {
            case ItemType.RUBI:
                itemName = "Rub�";
                description = "Mineral robusto extremadamente resistente.";
                itemType = "Material";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_rubi");
                break;

            case ItemType.DIAMANTE:
                itemName = "Diamante";
                description = "Mineral con incre�bles propiedades para el combate.";
                itemType = "Material";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/texture_diamond");
                break;

            case ItemType.POLVORA:
                itemName = "P�lvora";
                description = "Mezcla explosiva muy vers�til.";
                itemType = "Material";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_polvora");
                break;

            case ItemType.AMATISTA:
                itemName = "Amatista";
                description = "Piedra preciosa con gran valor.";
                itemType = "Material";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_amatista");
                break;

            case ItemType.CARBON:
                itemName = "Carb�n";
                description = "Mineral con valor medio en el mercado.";
                itemType = "Material";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_carbon");
                break;

            case ItemType.SALIVA:
                itemName = "Saliva";
                description = "L�quido biol�gico usado en las p�cimas.";
                itemType = "Ap�ndice";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_saliva");
                break;

            case ItemType.DIENTE:
                itemName = "Colmillo";
                description = "Colmillo con una resistencia sorprendente.";
                itemType = "Ap�ndice";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_diente");
                break;

            case ItemType.GARRA:
                itemName = "Garra";
                description = "Afilada garra capacitada para rajar piedras.";
                itemType = "Ap�ndice";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/item_Garra");
                break;

            case ItemType.OJO:
                itemName = "Ojo";
                description = "Globo ocular bastante asqueroso.";
                itemType = "Ap�ndice";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/texture_eye");
                break;

            case ItemType.HUESO:
                itemName = "Hueso";
                description = "Hueso de osiris, bastante resistente.";
                itemType = "Ap�ndice";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_hueso");
                break;

            case ItemType.VISCERA:
                itemName = "V�sceras";
                description = "V�sceras con propiedades en la alquimia.";
                itemType = "Ap�ndice";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_visceras");
                break;

            case ItemType.COLA:
                itemName = "Cola";
                description = "Cola sin propiedades especiales, puede valer dinero.";
                itemType = "Ap�ndice";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_cola");
                break;

            case ItemType.POCION_VIDA_1:
                itemName = "P�cima";
                description = "Curiosa p�cima que revitaliza la salud levemente.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_1");
                break;

            case ItemType.POCION_VIDA_2:
                itemName = "P�cima";
                description = "Curiosa p�cima que revitaliza buena parte de la salud.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_2");
                break;

            case ItemType.POCION_VIDA_3:
                itemName = "P�cima";
                description = "Curiosa p�cima que revitaliza gran parte de la salud.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_3");
                break;

            case ItemType.POCION_VIDA_MAX:
                itemName = "P�cima";
                description = "Curiosa p�cima que revitaliza la salud por completo.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_vida_max");
                break;

            case ItemType.POCION_REGENERACION:
                itemName = "P�cima";
                description = "Curiosa p�cima que aumenta tu regeneraci�n de manera moment�nea.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_regeneracion");
                break;

            case ItemType.POCION_DANO:
                itemName = "P�cima";
                description = "Curiosa p�cima que aumenta tu fuerza de manera moment�nea.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_dano");
                break;

            case ItemType.POCION_VELOCIDAD:
                itemName = "P�cima";
                description = "Curiosa p�cima que aumenta tu velocidad de manera moment�nea.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_pocion_velocidad");
                break;

            case ItemType.ORBE_MAGICO:
                itemName = "Orbe M�gico";
                description = "Objeto m�gico con propiedades sorprendentes.";
                itemType = "Consumible";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_orbe_magico");
                break;

            case ItemType.ZAFIRO:
                itemName = "Zafiro";
                description = "Mineral afilado con propiedades para el combate.";
                itemType = "Material";
                sr.sprite = Resources.Load<Sprite>("Textures/Items/textura_zafiro");
                break;

            case ItemType.BASURA:
                itemName = "Basura";
                description = "No parece que sirva para nada.";
                itemType = "Pez";
                sr.sprite = null; // en XML pon�a "---"
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
