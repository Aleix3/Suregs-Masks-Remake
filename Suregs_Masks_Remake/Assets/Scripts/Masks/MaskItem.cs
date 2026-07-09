using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskItem : MonoBehaviour
{
    public MaskData maskData;

    public void GetMask()
    {
        DialogueData dialogue = this.GetComponent<NPCInteractable>().currentDialogue;
        if (dialogue.sentences.Count == 0)
            return;
        DialogueManager.Instance.StartDialogue(
            dialogue,
            this.GetComponent<NPCInteractable>()
        );
        maskData.isUnlocked = true;
        Destroy(this.gameObject);
    }
}
