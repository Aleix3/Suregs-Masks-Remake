using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhaseDialogue
{
    public StoryPhase phase;
    [TextArea(3, 6)]
    public List<string> lines;
}
[CreateAssetMenu(menuName = "Dialogue/NPC Dialogue")]
public class NPCDialogueData : ScriptableObject
{
    public List<PhaseDialogue> dialogues;
}

