using System.Collections.Generic;

public static class QuestDatabase
{

    public static readonly List<QuestStep> MainQuestLine = new List<QuestStep>
    {
        new QuestStep("1",  "Habla con Vhea.", //done
            "Comienzo de la historia"),
        new QuestStep("2",  "Lucha en el campo de entrenamiento.", //done
            "Después de hablar con Vhea"),
        new QuestStep("3",  "Ve a casa y habla con Vhea.", //done
            "Después de perder en el Tutorial"),
        new QuestStep("4",  "Abre el cofre del dormitorio.", //done
            "Después de hablar con Vhea"),
        new QuestStep("5",  "Ve a las ruinas y descubre que es el artefacto del que hablaba tu padre.", //done
            "Después de abrir el cofre"),
        new QuestStep("6",  "Ve al árbol mágico a equiparte la máscara.", //done
            "Después de conseguir la máscara"),
        new QuestStep("7",  "Mira la tienda de Phrumo, el vendedor.", //done
            "Después del árbol"),
        new QuestStep("8",  "Mira la tienda de Zhyuka, la bruja.", //done
            "Después de hablar con Phrumo"),
        new QuestStep("9",  "Mira la herreria de Phoska, la herrera.", //done
            "Después de hablar con Zhyuka"),
        new QuestStep("10", "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.", //done
            "Después de la herrera"),
        new QuestStep("11", "Ve a casa y habla con Vhea.", //done
            "Después de la mazmorra 1"),
        new QuestStep("12", "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.", //done
            "Después de hablar con la abuela"),
        new QuestStep("13", "Ve a casa y habla con Vhea.", //done
            "Después de la mazmorra 2"),
        new QuestStep("14", "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.", //done
            "Después de hablar con la abuela"),
        new QuestStep("15", "Dale la gema extraña a Zhyuka.",//done
            "Después de la mazmorra 3"),
        new QuestStep("16", "Ve a casa y habla con Vhea.", //done
            "Después de darle la gema"),
        new QuestStep("17", "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.", //done
            "Después de hablar con la abuela"),
        new QuestStep("18", "Ve a casa y habla con Vhea.", //done
            "Después de la mazmorra 4"),
        new QuestStep("19", "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.", //done
            "Después de hablar con la abuela"),
        new QuestStep("20", "Ve a casa y habla con Vhea.", //done
            "Después de la mazmorra 5"),
        new QuestStep("21", "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.", //done
            "Después de hablar con la abuela"),
        new QuestStep("22", "Visita la herreria para equiparte.", //done
            "Después de la mazmorra 6"),
        new QuestStep("23", "Visita la tienda de pociones para equiparte.", //done
            "Después de la mazmorra 6"),
        new QuestStep("24", "Ve a casa y habla con Vhea.", //done
            "Después de visitar todas las tiendas"),
        new QuestStep("25", "Vuelve a las mazmorras y sigue descubriendo la historia de tu padre.", //done
            "Después de hablar con la abuela"),
    };


    public static readonly List<QuestStep> SideQuests = new List<QuestStep>
    {
        new QuestStep("2.1", "Dale la carta de Sukhy a Zupho, el guardia.",
            "Después de la mazmorra 0, en paralelo con la misión 6, tras hablar con Sukhy (diálogo 703)",
            isSideQuest: true),

        new QuestStep("2.2", "Entrega completada: el guardia se retira.",
            "Al hablar con el guardia tras darle la carta (se completa automáticamente)",
            isSideQuest: true),

        new QuestStep("3.1", "Termina el entrenamiento y lleva el paquete de Hemyl a Vhea. Después, dale la carta de Sukhy a Zupho, el guardia.",
            "Variante del inicio: si al salir de casa hablas con el vecino (diálogo 301)",
            isSideQuest: true),
    };

    public static QuestStep FindMainStep(string id)
    {
        return MainQuestLine.Find(step => step.id == id);
    }

    public static QuestStep FindSideQuest(string id)
    {
        return SideQuests.Find(step => step.id == id);
    }
}
