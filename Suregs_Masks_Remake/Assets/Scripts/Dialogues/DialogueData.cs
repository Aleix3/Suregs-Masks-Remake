using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string npcName;
    public Sprite portrait;

    public List<DialogueSentence> sentences;
}