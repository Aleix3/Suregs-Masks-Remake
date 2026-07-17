using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Mueve una Image (hoverImage) hasta la posición del botón seleccionado
/// actualmente en el EventSystem, con una animación suave de DOTween.
/// Ponlo en un GameObject vacío dentro de tu Canvas del menú.
///
/// Se pausa automáticamente mientras el menú principal está bloqueado
/// (mismo CanvasGroup que usas como "Block While Open" en los
/// UIRevealAnimator), para que no reaccione a lo que se selecciona dentro
/// de Ajustes/Controles/Créditos ni se líe durante la animación de cierre.
/// </summary>
public class MenuHoverIndicator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private RectTransform hoverImage;
    [SerializeField] private GameObject firstSelected;

    [Header("Bloqueo mientras hay un panel abierto")]
    [Tooltip("El mismo Transform que contiene los botones del menú principal (p.ej. 'Buttons'). Se ignora cualquier selección que no sea hija de este contenedor.")]
    [SerializeField] private Transform menuButtonsContainer;
    [Tooltip("El mismo CanvasGroup que usas como 'Block While Open' en los UIRevealAnimator. Mientras esté no-interactable (panel abierto), este script no hace nada.")]
    [SerializeField] private CanvasGroup menuButtonsCanvasGroup;

    [Header("Animación")]
    [SerializeField] private float moveDuration = 0.25f;
    [SerializeField] private Ease moveEase = Ease.OutQuad;
    [SerializeField] private bool matchWidth = true;
    [SerializeField] private bool matchHeight = false;

    [Header("Ajuste de posición")]
    [Tooltip("Desplazamiento (en píxeles de UI) que se suma a la posición del botón seleccionado. Usa esto para descentrar el hover si no encaja perfecto con tu sprite.")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    private GameObject lastSelected;
    private Tween moveTween;
    private Tween sizeTween;
    private bool wasBlocked;

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
        bool isBlocked = menuButtonsCanvasGroup != null && !menuButtonsCanvasGroup.interactable;

        if (isBlocked)
        {
            // Mientras haya un panel abierto no tocamos nada: ni movemos el
            // hover, ni forzamos selección. Solo recordamos que estábamos
            // bloqueados para resincronizar en cuanto se desbloquee.
            wasBlocked = true;
            return;
        }

        if (wasBlocked)
        {
            // Se acaba de cerrar el panel: nos colocamos al instante sobre el
            // botón que quedó seleccionado, sin animar (evita el salto raro).
            wasBlocked = false;
            moveTween?.Kill();
            sizeTween?.Kill();
            SnapToSelected();
            return;
        }

        GameObject current = EventSystem.current.currentSelectedGameObject;

        // Si se pierde la selección (p.ej. click fuera), volvemos a la última
        if (current == null)
        {
            if (lastSelected != null)
                EventSystem.current.SetSelectedGameObject(lastSelected);
            return;
        }

        // Ignoramos selecciones que no pertenezcan a los botones del menú principal
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

        hoverImage.position = target.position + (Vector3)offset;

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

        moveTween = hoverImage.DOMove(target.position + (Vector3)offset, moveDuration).SetEase(moveEase);

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