using UnityEngine;
using static Item;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] public DialogueData currentDialogue;

    public bool LockPlayerMovement => true;
    private bool alreadyGivenAmatist = false;

    public void Interact(Player player)
    {
        if (currentDialogue == null || currentDialogue.sentences.Count == 0)
        {
            
            return;
        }

        if (currentDialogue.name == "Zhyuka" && InventoryManager.instance.HasItem(ItemType.AMATISTA) && !alreadyGivenAmatist)
        {
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowSimpleMessage("Zhyuka", "Me das esta gema? muchas gracias!, ahora podré hacer pociones con muchos menos materiales!", currentDialogue.portrait, true);
                QuestManager.Instance.CompleteMainStepById("15");
                alreadyGivenAmatist = true;
                return;
            }
            
        }
        DialogueManager.Instance.StartDialogue(
            currentDialogue,
            this
        );

        if(currentDialogue.name == "Vhea")
        {
            QuestManager.Instance.CompleteMainStepById("1");
            QuestManager.Instance.CompleteMainStepById("3");
            QuestManager.Instance.CompleteMainStepById("11");
            QuestManager.Instance.CompleteMainStepById("13");
            QuestManager.Instance.CompleteMainStepById("16");
            QuestManager.Instance.CompleteMainStepById("18");
            QuestManager.Instance.CompleteMainStepById("20");
            QuestManager.Instance.CompleteMainStepById("24");
        }

        
    }

    public void StopInteract(Player player)
    {
        //DialogueManager.Instance.EndDialogue();

        if (LockPlayerMovement)
            player.canMove = true;
    }
}