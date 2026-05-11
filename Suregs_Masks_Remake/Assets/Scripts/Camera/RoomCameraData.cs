using UnityEngine;

public class RoomCameraData : MonoBehaviour
{
    public bool followPlayer = true;
    public Collider2D confinerShape;

    [Header("Camera settings")]
    public float orthographicSize = 5f;

    [Header("Optional fixed camera target")]
    public Transform cameraAnchor;
}