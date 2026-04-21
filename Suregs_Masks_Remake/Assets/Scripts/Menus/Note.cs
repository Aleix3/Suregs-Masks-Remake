using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class Note : MonoBehaviour
{
    public int id;
    public string itemName;
    public string description;

    void Start()
    {
        (itemName, description) = GetItemData(id);
    }

    public static (string itemName, string description) GetItemData(int id)
    {
        string name = "";
        string desc = "";

        switch (id)
        {
            case 1:
                name = "Día 2";
                desc = "Nos han vuelto a atacar los süregs, se han llevado a Alma… No me creo que algo así te haya pasado… Debe haber alguna manera de traerte de vuelta… Prometo que encontraré…";
                break;

            case 2:
                name = "Día 5";
                desc = "Hemos descubierto que estos seres vienen de las antiguas ruinas cerca del pueblo...";
                break;

            case 3:
                name = "Día 7";
                desc = "Los guerreros más fuertes preparamos una expedición...";
                break;

            case 4:
                name = "Día 11";
                desc = "Casi una semana más tarde conseguimos volver a casa...";
                break;

            case 5:
                name = "Día 16";
                desc = "Después de descansar unos días en el pueblo...";
                break;

            case 6:
                name = "Día 20";
                desc = "Seguimos adentrándonos en las ruinas...";
                break;

            case 7:
                name = "Día 23";
                desc = "Huhan: Esta noche he notado que alguien me tiraba del pie...";
                break;

            case 8:
                name = "Día 27";
                desc = "Hemos descubierto el poder detrás de los süreg...";
                break;

            case 9:
                name = "Día 28";
                desc = "Fukho: Cada día que pasa nos llevamos peor...";
                break;

            case 10:
                name = "Día ???";
                desc = "Igorv: No sé cuántos días llevo aquí abajo...";
                break;

            default:
                name = "Desconocido";
                desc = "No hay datos para este ID.";
                break;
        }

        return (name, desc);
    }
}
