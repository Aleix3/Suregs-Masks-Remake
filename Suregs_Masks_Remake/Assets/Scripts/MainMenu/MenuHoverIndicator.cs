using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Mueve una Image (hoverImage) hasta la posición del botón seleccionado
/// actualmente en el EventSystem, con una animación suave de DOTween.
/// Ponlo en un GameObject vacío dentro de tu Canvas del menú.
/// </summary>
public class MenuHoverIndicator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform hoverImage;
    [SerializeField] private GameObject firstSelected;

    [Header("Animación")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private bool matchWidth = true;
    [SerializeField] private bool matchHeight = false;

    private GameObject lastSelected;
    private Tween moveTween;
    private Tween sizeTween;

    private void Start()
    {
        // Aseguramos que siempre haya algo seleccionado (necesario para mando/teclado)
        if (EventSystem.current.currentSelectedGameObject == null && firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected);
        }

        SnapToSelected();
    }

    private void Update()
    {
        GameObject current = EventSystem.current.currentSelectedGameObject;

        // Si se pierde la selección (p.ej. click fuera), volvemos a la última
        if (current == null)
        {
            if (lastSelected != null)
                EventSystem.current.SetSelectedGameObject(lastSelected);
            return;
        }

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

        hoverImage.position = target.position;

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

        moveTween = hoverImage.DOMove(target.position, moveDuration).SetEase(moveEase);

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