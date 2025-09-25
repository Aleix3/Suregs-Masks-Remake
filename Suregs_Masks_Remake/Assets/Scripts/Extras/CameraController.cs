using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform player;          // referencia al jugador
    public float smooth;

    private Vector3 targetPosition;

    void Update()
    {
        targetPosition.x = player.position.x;
        targetPosition.y = player.position.y;
        targetPosition.z = this.transform.position.z;

        this.transform.position = Vector3.Lerp(this.transform.position, targetPosition, smooth*Time.deltaTime);
    }


}
