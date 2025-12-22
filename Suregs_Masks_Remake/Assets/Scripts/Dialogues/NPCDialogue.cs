using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public NPCDialogueData dialogueData;

    public void Interact()
    {
        StoryPhase phase = StoryManager.Instance.currentPhase;
        PhaseDialogue dialogue = dialogueData.dialogues
            .Find(d => d.phase == phase);

        DialogueManager.Instance.StartDialogue(dialogue.lines);
    }
}

