using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Item;

public class NoteItem : MonoBehaviour
{
    
    public int id;
    int ObjectId = -1;
    public new string name;
    public Image itemImage;
    bool inList = false;
    public string description;

    private void Awake()
    {
        itemImage = GetComponent<Image>();
    }


}
