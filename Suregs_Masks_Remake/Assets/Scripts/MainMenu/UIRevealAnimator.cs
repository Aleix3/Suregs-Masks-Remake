using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Componente reutilizable para animar entrada/salida de un panel o imagen.
/// Ponlo en el mismo GameObject que el Panel/Image (necesita CanvasGroup).
/// Cambia "revealType" en el inspector para variar la animación entre
/// Ajustes / Controles / Créditos.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(RectTransform))]
public class UIRevealAnimator : MonoBehaviour
{
    public enum RevealType
    {
        ScaleFade,          // Aparece creciendo desde el centro (ideal para el panel de Ajustes)
        SlideFromRight,     // Entra deslizando desde la derecha (ideal para Controles)
        SlideFromLeftRotate // Entra desde la izquierda con una leve rotación (ideal para Créditos)
    }

    [Header("Tipo de animación")]
    [SerializeField] private RevealType revealType = RevealType.ScaleFade;

    [Header("Configuración")]
    [SerializeField] private float duration = 0.4f;
    [SerializeField] private Ease easeIn = Ease.OutBack;
    [SerializeField] private Ease easeOut = Ease.InBack;
    [SerializeField] private bool startHidden = true;
    [SerializeField] private float slideDistance = 300f;

    [Header("Navegación (EventSystem)")]
    [Tooltip("Elemento que se seleccionará automáticamente al abrir este panel (p.ej. el primer Slider o el botón Cerrar).")]
    [SerializeField] private GameObject firstSelectedOnOpen;
    [Tooltip("CanvasGroup del menú/panel que hay DETRÁS (p.ej. los botones del MainMenu). Se desactiva mientras este panel está abierto para que no se pueda navegar por él.")]
    [SerializeField] private CanvasGroup blockWhileOpen;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 originalPos;
    private Vector3 originalScale;
    private Sequence currentSequence;
    private GameObject previousSelected;

    /// <summary>True mientras el panel está visible o animándose hacia visible.</summary>
    public bool IsOpen { get; private set; }

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPos = rect.anchoredPosition;
        originalScale = rect.localScale;

        if (startHidden)
            gameObject.SetActive(false);
    }

    public void Show()
    {
        currentSequence?.Kill();
        gameObject.SetActive(true);
        IsOpen = true;

        // Guardamos qué había seleccionado antes (para restaurarlo al cerrar)
        // y bloqueamos el menú de detrás para que no se pueda navegar por él.
        previousSelected = EventSystem.current.currentSelectedGameObject;
        if (blockWhileOpen != null)
        {
            blockWhileOpen.interactable = false;
            blockWhileOpen.blocksRaycasts = false;
        }
        EventSystem.current.SetSelectedGameObject(null);

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0f;

        currentSequence = DOTween.Sequence();

        switch (revealType)
        {
            case RevealType.ScaleFade:
                rect.localScale = originalScale * 0.85f;
                rect.anchoredPosition = originalPos;
                rect.localRotation = Quaternion.identity;
                currentSequence.Join(rect.DOScale(originalScale, duration).SetEase(easeIn));
                break;

            case RevealType.SlideFromRight:
                rect.localScale = originalScale;
                rect.localRotation = Quaternion.identity;
                rect.anchoredPosition = originalPos + new Vector2(slideDistance, 0f);
                currentSequence.Join(rect.DOAnchorPos(originalPos, duration).SetEase(easeIn));
                break;

            case RevealType.SlideFromLeftRotate:
                rect.localScale = originalScale;
                rect.anchoredPosition = originalPos + new Vector2(-slideDistance, 0f);
                rect.localRotation = Quaternion.Euler(0f, 0f, -12f);
                currentSequence.Join(rect.DOAnchorPos(originalPos, duration).SetEase(easeIn));
                currentSequence.Join(rect.DOLocalRotate(Vector3.zero, duration).SetEase(easeIn));
                break;
        }

        currentSequence.Join(canvasGroup.DOFade(1f, duration));
        currentSequence.OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            if (firstSelectedOnOpen != null)
                EventSystem.current.SetSelectedGameObject(firstSelectedOnOpen);
        });
    }

    public void Hide()
    {
        currentSequence?.Kill();
        IsOpen = false;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        currentSequence = DOTween.Sequence();

        switch (revealType)
        {
            case RevealType.ScaleFade:
                currentSequence.Join(rect.DOScale(originalScale * 0.85f, duration).SetEase(easeOut));
                break;

            case RevealType.SlideFromRight:
                currentSequence.Join(rect.DOAnchorPos(originalPos + new Vector2(slideDistance, 0f), duration).SetEase(easeOut));
                break;

            case RevealType.SlideFromLeftRotate:
                currentSequence.Join(rect.DOAnchorPos(originalPos + new Vector2(-slideDistance, 0f), duration).SetEase(easeOut));
                currentSequence.Join(rect.DOLocalRotate(new Vector3(0f, 0f, -12f), duration).SetEase(easeOut));
                break;
        }

        currentSequence.Join(canvasGroup.DOFade(0f, duration));
        currentSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            rect.anchoredPosition = originalPos;
            rect.localScale = originalScale;
            rect.localRotation = Quaternion.identity;

            if (blockWhileOpen != null)
            {
                blockWhileOpen.interactable = true;
                blockWhileOpen.blocksRaycasts = true;
            }
            EventSystem.current.SetSelectedGameObject(previousSelected);
        });
    }
}