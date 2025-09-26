using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class InventoryItem : MonoBehaviour
{
    public ItemType type;
    int id;
    int ObjectId = -1;
    public new string name;
    public string itemType;
    public Image itemImage;
    public Image closeUpItem;
    public uint quantity = 1;
    public bool stackable = false;
    bool inList = false;
    public string description;

    private void Awake()
    {
        itemImage = GetComponent<Image>();
        closeUpItem = GetComponent<Image>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
