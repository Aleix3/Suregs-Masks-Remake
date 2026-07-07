using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaveEnterScript : MonoBehaviour
{
    private bool changingScene = false;
    public GameObject grandmaSpawnPoint;
    public GameObject caveSpawnPoint;
    public CinemachineConfiner2D confiner;
    public RoomCameraData townRoom;

    private void Start()
    {
        if(Player.Instance.spawnPointChanged)
        {
            grandmaSpawnPoint.SetActive(false);
            caveSpawnPoint.SetActive(true);
            confiner.m_BoundingShape2D = townRoom.confinerShape;
            townRoom.GetComponent<Room>().isPlayerInRoom = true;
            confiner.GetComponent<CinemachineVirtualCamera>().Follow = Player.Instance.transform;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (changingScene) return;

        if (collision.CompareTag("Player"))
        {
            changingScene = true;
            StartCoroutine(ChangeScene(collision));
        }
    }

    IEnumerator ChangeScene(Collider2D collision)
    {
        yield return StartCoroutine(CameraManager.Instance.Fade(1));
        if(Player.Instance.isFacingLeft)
        {
            collision.transform.localScale = new Vector3(0.7245f, 0.7245f, 0.7245f);
        }
        else
        {
            collision.transform.localScale = new Vector3(-0.7245f, 0.7245f, 0.7245f);
        }
        
        Player.Instance.spawnPointChanged = true;
        SceneManager.LoadScene("Dungeon 0");
    }


}
