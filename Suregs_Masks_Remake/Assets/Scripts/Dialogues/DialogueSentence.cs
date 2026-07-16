using System.Collections.Generic;
using UnityEngine;

public enum SentenceType
{
    Normal,
    Choice,
    Commerce,
    QuestUpdate,
    CombatTutorial
}

[System.Serializable]
public class DialogueSentence
{
    [TextArea]
    public string text;
    public string id;

    public SentenceType type;

    public GameProgressState minState;

    public List<DialogueOption> options;

    public int commerceID;

    [Header("Override Speaker")]
    public bool overrideSpeaker;

    public string speakerName;
    public Sprite speakerPortrait;
}