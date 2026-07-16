using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;
using UnityEngine;


public class Chest : MonoBehaviour, IInteractable
{
    public GameObject chestClosed;
    public GameObject chestOpened;
    public BoxCollider2D trigger;
    public GameObject itemPrefab;

    public bool LockPlayerMovement => false;
    private Player currentPlayer;


    public void Interact(Player player)
    {
        currentPlayer = player;

        chestOpened.SetActive(true);
        chestClosed.SetActive(false);
        trigger.enabled = false;
        DropItems();
    }

    public void StopInteract(Player player)
    {
        
    }

    public void DropItems()
    {
        // Suelta siempre 2 objetos
        DropPrimaryItem();
        DropSecondaryItem();

        // 15% probabilidad de soltar un extra
        if (Random.Range(0, 100) < 15)
        {
            DropSecondaryItem();
        }
    }

    // PRIMER OBJETO (Amatista / Rubí / Carbón)

    void DropPrimaryItem()
    {
        int r = Random.Range(0, 101);

        Item.ItemType type;

        if (r < 35)
            return;
        else
            type = Item.ItemType.CARBON;

        SpawnItem(type);
    }


    // SEGUNDO OBJETO (Pociones)

    void DropSecondaryItem()
    {
        int r = Random.Range(0, 101);
        Item.ItemType type;

        if (r >= 17 && r < 32)
            type = Item.ItemType.POCION_DANO;
        else if (r >= 47 && r < 63)
            type = Item.ItemType.POCION_VELOCIDAD;
        else if (r >= 3 && r < 17)
            type = Item.ItemType.POCION_REGENERACION;
        else if (r >= 81 && r <= 100)
            type = Item.ItemType.POCION_VIDA_1;
        else if (r >= 63 && r < 81)
            type = Item.ItemType.POCION_VIDA_2;
        else if (r >= 32 && r < 47)
            type = Item.ItemType.POCION_VIDA_3;
        else if (r >= 0 && r < 3)
            type = Item.ItemType.POCION_VIDA_MAX;
        else
            return;

        SpawnItem(type);
    }

    void SpawnItem(Item.ItemType type)
    {
        GameObject newItem = Instantiate(itemPrefab, transform.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.3f, 0f), Quaternion.identity);
        Item item = newItem.GetComponent<Item>();
        item.type = type;
        newItem.AddComponent<ItemSpawnAnim>();
    }


}
