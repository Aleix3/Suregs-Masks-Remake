using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public RoomCameraData roomData;
    public Transform roomTriggerConnected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || other.isTrigger) return;


        float distance = 1.5f;

        Vector3 positionToSpawn = other.transform.position = roomTriggerConnected.position + (roomTriggerConnected.position - transform.position).normalized * distance;



        CameraManager.Instance.TransitionToRoom(
            roomData,
            positionToSpawn
        );
        this.transform.parent.GetComponent<Room>().isPlayerInRoom = true;
        Player.Instance.actualRoom = this.transform.parent.GetComponent<Room>();
        roomTriggerConnected.parent.GetComponent<Room>().isPlayerInRoom = false;
    }
}