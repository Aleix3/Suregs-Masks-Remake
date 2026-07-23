using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CinematicPlayer : MonoBehaviour
{

    public string resourcesFolderPath = "Cinematica/Intro";

    [Header("Referencias UI")]
    public Image displayImage;
    public Image skipProgressBar;

    [Header("Tiempos")]
    public float timePerImage = 0.15f;
    public float holdToSkipDuration = 1.5f;

    public string submitButtonName = "Submit";

    public string nextSceneName = "Gameplay";

    private Sprite[] cinematicImages;
    private float holdTimer = 0f;
    private bool isSkipping = false;
    private Coroutine playbackCoroutine;

    public AudioClip cinematicSFX;

    void Start()
    {
        // Carga y ordena numéricamente por nombre tipo "Cinematic X_Y" (grupo X, índice Y)
        cinematicImages = Resources.LoadAll<Sprite>(resourcesFolderPath)
                                    .OrderBy(s => GetSortKey(s.name).group)
                                    .ThenBy(s => GetSortKey(s.name).index)
                                    .ToArray();

        if (cinematicImages.Length == 0)
        {
            Debug.LogError($"No se encontraron sprites en Resources/{resourcesFolderPath}");
            LoadNextScene();
            return;
        }

        // Log de diagnóstico: imprime el orden final calculado.
        // Revisa la Consola al arrancar la escena para confirmar que el orden es el correcto.
        Debug.Log($"Cinemática cargada: {cinematicImages.Length} sprites. Orden calculado:\n" +
                   string.Join(" -> ", cinematicImages.Select(s => s.name)));

        if (skipProgressBar != null)
            skipProgressBar.fillAmount = 0f;

        playbackCoroutine = StartCoroutine(PlayCinematic());
    }

    void Update()
    {
        if (Input.GetButton(submitButtonName))
        {
            holdTimer += Time.deltaTime;

            if (skipProgressBar != null)
                skipProgressBar.fillAmount = holdTimer / holdToSkipDuration;

            if (holdTimer >= holdToSkipDuration && !isSkipping)
            {
                isSkipping = true;
                SkipCinematic();
            }
        }
        else
        {
            holdTimer = 0f;

            if (skipProgressBar != null)
                skipProgressBar.fillAmount = 0f;
        }
    }


    private static readonly Regex sortRegex = new Regex(@"^Cinematic\s*(\d+)_(\d+)$", RegexOptions.IgnoreCase);

    (int group, int index) GetSortKey(string spriteName)
    {
        Match match = sortRegex.Match(spriteName.Trim());

        if (match.Success)
        {
            int group = int.Parse(match.Groups[1].Value);
            int index = int.Parse(match.Groups[2].Value);
            return (group, index);
        }

        Debug.LogWarning($"El sprite '{spriteName}' no coincide EXACTAMENTE con el patrón 'Cinematic X_Y'. Se colocará al final. Revisa este nombre en el Project window.");
        return (int.MaxValue, int.MaxValue);
    }

    IEnumerator PlayCinematic()
    {
        AudioManager.Instance.PlaySFX(cinematicSFX);
        for (int i = 0; i < cinematicImages.Length; i++)
        {
            displayImage.sprite = cinematicImages[i];
            yield return new WaitForSeconds(timePerImage);
        }

        LoadNextScene();
    }

    void SkipCinematic()
    {
        if (playbackCoroutine != null)
            StopCoroutine(playbackCoroutine);

        LoadNextScene();
    }

    void LoadNextScene()
    {
        StartCoroutine(FadeAndLoadScene(nextSceneName));

    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(CameraManager.Instance.Fade(1));

        
        SceneManager.LoadScene(sceneName);
    }
}