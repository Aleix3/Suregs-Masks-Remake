using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MaskPointNotification : MonoBehaviour
{
    public Image              maskIcon;
    public TextMeshProUGUI    label;

    [Header("Animación")]
    public float slideInDuration  = 0.25f;
    public float holdDuration     = 1.5f;
    public float fadeOutDuration  = 0.4f;
    public float slideDistance    = 60f;

    private CanvasGroup _cg;
    private RectTransform _rt;

    private void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();
    }


    public void Show(Sprite icon, string text)
    {
        if (maskIcon != null)
        {
            maskIcon.enabled = icon != null;

            if (icon != null)
            {
                maskIcon.sprite = icon;
                maskIcon.preserveAspect = true;
            }
        }

        if (label != null)
            label.text = text;

        StartCoroutine(AnimRoutine());
    }

    private IEnumerator AnimRoutine()
    {
        Vector2 endPos   = _rt.anchoredPosition;
        Vector2 startPos = endPos - new Vector2(0, slideDistance);

        // Slide in
        float t = 0f;
        while (t < slideInDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / slideInDuration);
            _rt.anchoredPosition = Vector2.Lerp(startPos, endPos, Mathf.SmoothStep(0, 1, p));
            if (_cg != null) _cg.alpha = p;
            yield return null;
        }
        _rt.anchoredPosition = endPos;
        if (_cg != null) _cg.alpha = 1f;

        // Hold
        yield return new WaitForSeconds(holdDuration);

        // Fade out
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            if (_cg != null) _cg.alpha = 1f - Mathf.Clamp01(t / fadeOutDuration);
            yield return null;
        }

        Destroy(gameObject);
    }
}
