using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleFadeToCredits : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup image;

    [Header("Timing")]
    public float fadeInDuration = 2f;
    public float holdDuration = 10f;
    public float fadeOutDuration = 2f;

    [Header("Scene")]
    public string nextSceneName = "Creditos";

    private void Start()
    {
        
        image.alpha = 0f;
        StartCoroutine(PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        yield return StartCoroutine(Fade(0f, 1f, fadeInDuration));

        yield return new WaitForSeconds(holdDuration);

        yield return StartCoroutine(Fade(1f, 0f, fadeOutDuration));
        AudioManager.Instance.PlayMusic(AudioManager.Instance.creditsMusic);
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        float elapsed = 0f;
        image.alpha = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            image.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        image.alpha = to;
    }
}