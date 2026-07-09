using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        MaskManager.Instance.UnlockMask(maskData);
        Destroy(this.gameObject);
    }
}
