using UnityEngine;
using Cinemachine;
using System.Collections;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance;

    [Header("References")]
    public CinemachineVirtualCamera virtualCamera;
    public Transform player;

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.5f;

    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        confiner = virtualCamera.GetComponent<CinemachineConfiner2D>();
    }

    void Start()
    {
        player = Player.Instance.transform;
    }

    public void TransitionToRoom(RoomCameraData room, Vector3 teleportPosition)
    {
        StartCoroutine(RoomRoutine(room, teleportPosition));
    }

    private IEnumerator RoomRoutine(RoomCameraData room, Vector3 teleportPosition)
    {
        // 1. FADE OUT
        yield return Fade(1);

        // 2. TELEPORT

        player.position = teleportPosition;

        // 3. ESPERA ESTABILIZACIÓN
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // 4. CAMBIO DE CÁMARA
        virtualCamera.Follow = room.followPlayer ? player : room.cameraAnchor;

        if (confiner != null && room.confinerShape != null)
        {
            confiner.m_BoundingShape2D = room.confinerShape;
            confiner.InvalidateCache();
        }

        virtualCamera.m_Lens.OrthographicSize = room.orthographicSize;

        // 5. FADE IN
        yield return Fade(0);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeImage == null) yield break;

        float startAlpha = fadeImage.color.a;
        float t = 0;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float lerp = t / fadeDuration;

            Color c = fadeImage.color;
            c.a = Mathf.Lerp(startAlpha, targetAlpha, lerp);
            fadeImage.color = c;

            yield return null;
        }
    }
}