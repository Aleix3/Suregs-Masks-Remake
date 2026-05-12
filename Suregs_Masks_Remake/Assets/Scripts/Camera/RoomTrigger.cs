using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    public RoomCameraData roomData;
    public Transform roomTriggerConnected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || other.isTrigger) return;

        //Vector2 dir = (other.transform.position - transform.position).normalized;

        //if (dir == Vector2.zero)
        //    dir = Vector2.up; 

        float distance = 1.5f;

        Vector3 positionToSpawn = other.transform.position = roomTriggerConnected.position + (roomTriggerConnected.position - transform.position).normalized * distance;



        CameraManager.Instance.TransitionToRoom(
            roomData,
            positionToSpawn
        );
    }
}