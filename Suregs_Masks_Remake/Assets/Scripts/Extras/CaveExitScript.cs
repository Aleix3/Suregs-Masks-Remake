using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CaveExitScript : MonoBehaviour
{
    private bool changingScene = false;

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
        if (Player.Instance.isFacingLeft)
        {
            collision.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
        }
        else
        {
            collision.transform.localScale = new Vector3(-0.55f, 0.55f, 0.55f);
        }
        
        SceneManager.LoadScene("Town");
    }


}
