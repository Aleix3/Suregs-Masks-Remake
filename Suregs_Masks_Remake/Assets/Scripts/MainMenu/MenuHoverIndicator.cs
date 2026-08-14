using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MenuHoverIndicator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform hoverImage;
    [SerializeField] private GameObject firstSelected;

    [Header("Bloqueo mientras hay un panel abierto")]
    [SerializeField] private Transform menuButtonsContainer;
    [SerializeField] private CanvasGroup menuButtonsCanvasGroup;

    [Header("Animación")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private bool matchWidth = true;
    [SerializeField] private bool matchHeight = false;

    [Header("Ajuste de posición")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    private GameObject lastSelected;
    private Tween moveTween;
    private Tween sizeTween;
    private bool wasBlocked;

    private Canvas rootCanvas;
    private Vector2Int lastScreenSize;

    private void Awake()
    {
        rootCanvas = hoverImage.GetComponentInParent<Canvas>().rootCanvas;
    }

    private Vector3 GetScaledOffset()
    {
        float scale = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        return (Vector3)(offset * scale);
    }

    private void Start()
    {
        if (EventSystem.current.currentSelectedGameObject == null && firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        SnapToSelected();
    }

    private void LateUpdate()
    {
        if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
        {
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            moveTween?.Kill();
            sizeTween?.Kill();
            Canvas.ForceUpdateCanvases();
            SnapToSelected();
            return;
        }

        bool isBlocked = menuButtonsCanvasGroup != null && !menuButtonsCanvasGroup.interactable;

        if (isBlocked)
        {
            wasBlocked = true;
            return;
        }

        if (wasBlocked)
        {
            wasBlocked = false;
            moveTween?.Kill();
            sizeTween?.Kill();
            SnapToSelected();
            return;
        }

        GameObject current = EventSystem.current.currentSelectedGameObject;

        if (current == null)
        {
            if (lastSelected != null)
                EventSystem.current.SetSelectedGameObject(lastSelected);
            return;
        }

        if (menuButtonsContainer != null && !current.transform.IsChildOf(menuButtonsContainer))
            return;

        if (current != lastSelected)
        {
            RectTransform target = current.GetComponent<RectTransform>();
            if (target != null)
            {
                MoveHoverTo(target);
                lastSelected = current;
            }
        }
    }

    private void SnapToSelected()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;
        if (current == null || hoverImage == null) return;

        RectTransform target = current.GetComponent<RectTransform>();
        if (target == null) return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(target);

        hoverImage.position = target.position + GetScaledOffset();

        Vector2 size = hoverImage.sizeDelta;
        if (matchWidth) size.x = target.rect.width;
        if (matchHeight) size.y = target.rect.height;
        hoverImage.sizeDelta = size;

        lastSelected = current;
    }

    private void MoveHoverTo(RectTransform target)
    {
        moveTween?.Kill();
        sizeTween?.Kill();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(target);

        moveTween = hoverImage.DOMove(target.position + GetScaledOffset(), moveDuration).SetEase(moveEase);

        if (matchWidth || matchHeight)
        {
            Vector2 targetSize = hoverImage.sizeDelta;
            if (matchWidth) targetSize.x = target.rect.width;
            if (matchHeight) targetSize.y = target.rect.height;

            sizeTween = DOTween.To(
                () => hoverImage.sizeDelta,
                v => hoverImage.sizeDelta = v,
                targetSize,
                moveDuration
            ).SetEase(moveEase);
        }
    }
}