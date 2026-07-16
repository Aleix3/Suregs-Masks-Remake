using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;
using UnityEngine;


public class BeedRoomChest : MonoBehaviour, IInteractable
{
    public BoxCollider2D trigger;
    public List <GameObject> itemsToSpawn = new List<GameObject>();

    public bool LockPlayerMovement => false;
    private Player currentPlayer;
    public Transform spawnItemsPos;

    private void Start()
    {
        DialogueManager.Instance.FindShops();
    }
    public void Interact(Player player)
    {
        if (QuestManager.Instance.CurrentMainQuest.id != "4")
            return;
        currentPlayer = player;
        trigger.enabled = false;
        DropItems();
        QuestManager.Instance.CompleteMainStepById("4");
    }

    public void StopInteract(Player player)
    {
        
    }

    public void DropItems()
    {
        float yOfsset = 0;
        for (int i = 0; i < itemsToSpawn.Count; i++)
        {
            Instantiate(itemsToSpawn[i]);
            itemsToSpawn[i].transform.position = new Vector3(spawnItemsPos.position.x, spawnItemsPos.position.y + yOfsset, spawnItemsPos.position.z);
            yOfsset += 0.5f;
            itemsToSpawn[i].GetComponent<Note>().id = i + 1;
        }
    }

    


}
