using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaskItem : MonoBehaviour
{
    public MaskData maskData;

    private void Awake()
    {
        if(maskData.isUnlocked)
        {
            Destroy(gameObject);
        }
    }
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
        switch (maskData.maskID)
        {   
            case 0:
                QuestManager.Instance.CompleteMainStepById("5");
                break;

        }
        Destroy(this.gameObject);
    }
}
