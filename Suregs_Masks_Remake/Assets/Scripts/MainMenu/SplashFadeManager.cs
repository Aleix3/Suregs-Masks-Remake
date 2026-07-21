using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashFadeManager : MonoBehaviour
{
    public CanvasGroup image1;
    public CanvasGroup image2;

    public float fadeInDuration = 1f;
    public float holdDuration = 1.5f;
    public float fadeOutDuration = 1f;

    public string nextSceneName = "MainMenu";

    void Start()
    {

        image1.alpha = 0f;
        image2.alpha = 0f;

        StartCoroutine(PlaySplashSequence());
    }

    IEnumerator PlaySplashSequence()
    {

        yield return StartCoroutine(Fade(image1, 0f, 1f, fadeInDuration));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(Fade(image1, 1f, 0f, fadeOutDuration));

        yield return StartCoroutine(Fade(image2, 0f, 1f, fadeInDuration));
        yield return new WaitForSeconds(holdDuration);
        yield return StartCoroutine(Fade(image2, 1f, 0f, fadeOutDuration));

        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator Fade(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        cg.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        cg.alpha = to;
    }
}
