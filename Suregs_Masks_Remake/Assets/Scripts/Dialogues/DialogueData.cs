using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    public string npcName;
    public Sprite portrait;
    public bool isPermanent = false;

    [Header("Comercio")]
    public bool isCommerceDialogue;

    public List<DialogueSentence> sentences;
}