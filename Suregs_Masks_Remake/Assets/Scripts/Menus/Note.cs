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
                desc = "Hemos descubierto que estos seres vienen de las antiguas ruinas cerca del pueblo, aunque estaban abandonadas, ahora están llenas de… vida? Parece que los süreg se están multiplicando, no paran de salir.";
                break;

            case 3:
                name = "Día 7";
                desc = "Los guerreros más fuertes preparamos una expedición para adentrarnos en las ruinas. Antes de partir necesitaremos mejorar el equipo en la herrería y comprar las pociones en el alquimista";
                break;

            case 4:
                name = "Día 11";
                desc = "Casi una semana más tarde conseguimos volver a casa, después de un arduo camino encontramos un artefacto que guarda una magia poderosa, es posible que nos ayude contra los süreg.";
                break;

            case 5:
                name = "Día 16";
                desc = "Después de descansar unos días en el pueblo, y aprovechando el poder de nuestra nueva máscara, decidimos volver a las ruinas pero nos ha sorprendido un obstáculo inesperado, no sabemos qué es pero es demasiado fuerte.";
                break;

            case 6:
                name = "Día 20";
                desc = "Seguimos adentrándonos en las ruinas, notamos una presencia que nos sigue de cerca, aunque ninguno de nosotros ha sido capaz de verla, todo este sitio parece que vigila nuestros pasos";
                break;

            case 7:
                name = "Día 23";
                desc = "Esta noche he notado que alguien me tiraba del pie, al despertarme solo estaba Igorv con la espada desenvainada gritando hacia la oscuridad, cuando le pregunté me dijo que no podía describir lo que acababa de ver, parecía muy alterado";
                break;

            case 8:
                name = "Día 27";
                desc = "Hemos descubierto el poder detrás de los süreg, estas máscaras, llevamos días usándolas contra ellos y son eficaces, aunque Hemyl jura escuchar susurros al usarla.";
                break;

            case 9:
                name = "Día 28";
                desc = "Cada día que pasa nos llevamos peor, hemos perdido el camino de vuelta… Igorv sigue empeñado en llegar al final de todo esto… está fuera de sí, nunca debimos entrar en estas malditas ruinas, daría lo que fuera por volver a ver la luz del sol.";
                break;

            case 10:
                name = "Día ???";
                desc = "INo sé cuántos días llevo aquí abajo, estoy perdiendo la cabeza, a veces no recuerdo lo que hago y no soy capaz de controlarme, he encontrado el cadáver de Fukho y de Hemyl, esa cosa los ha matado, es mi turno.";
                break;

            default:
                name = "Desconocido";
                desc = "No hay datos para este ID.";
                break;
        }

        return (name, desc);
    }
}
