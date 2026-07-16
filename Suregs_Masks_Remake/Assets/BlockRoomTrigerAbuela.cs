using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockRoomTrigerAbuela : MonoBehaviour
{
    public string messageSpeaker;
    public string message;
    public Sprite speakerSprite;
    public BoxCollider2D colliderToEnable;

    // Update is called once per frame
    void Update()
    {
        if(QuestManager.Instance.CurrentMainQuest.id == "2")
        {
            colliderToEnable.enabled = true;
            Destroy(this.gameObject);
        }
        else
        {
            colliderToEnable.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            DialogueManager.Instance.ShowSimpleMessage(messageSpeaker, message, speakerSprite);
        }
    }
}
