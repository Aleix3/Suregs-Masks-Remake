using UnityEngine;
using UnityEngine.EventSystems;


public class UpgradeNodeButton : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("Índice del nodo (0-15)")]
    public int nodeIndex;

    private MaskTreeUI _ui;

    private void Start()
    {
        _ui = GetComponentInParent<MaskTreeUI>();
    }

    public void OnPointerEnter(PointerEventData eventData)
        => _ui?.OnNodeHoverEnter(nodeIndex);

    public void OnPointerExit(PointerEventData eventData)
        => _ui?.OnNodeHoverExit();
}
