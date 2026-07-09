using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] public DialogueData currentDialogue;

    public bool LockPlayerMovement => true;

    public void Interact(Player player)
    {
        if (currentDialogue.sentences.Count == 0)
            return;
        DialogueManager.Instance.StartDialogue(
            currentDialogue,
            this
        );


            
    }

    public void StopInteract(Player player)
    {
        //DialogueManager.Instance.EndDialogue();

        if (LockPlayerMovement)
            player.canMove = true;
    }
}