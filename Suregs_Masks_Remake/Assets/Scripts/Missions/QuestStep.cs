using System;
using UnityEngine;


[Serializable]
public class QuestStep
{
    public string id;

    [Tooltip("Texto que se muestra en el HUD.")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("Nota interna sobre cuándo se activa/completa este paso. NO se muestra en el HUD, es solo referencia para diseño/depuración.")]
    [TextArea(1, 3)]
    public string triggerHint;

    [Tooltip("Si es true, este paso es una misión secundaria (puede coexistir con la misión principal actual).")]
    public bool isSideQuest;

    public QuestStep(string id, string description, string triggerHint = "", bool isSideQuest = false)
    {
        this.id = id;
        this.description = description;
        this.triggerHint = triggerHint;
        this.isSideQuest = isSideQuest;
    }
}
