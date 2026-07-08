using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeNodeButton :MonoBehaviour, ISelectHandler,
    IDeselectHandler

{
    public int nodeIndex;

    private MaskTreeUI ui;

    [SerializeField] public GameObject hover;

    void Awake()
    {
        ui = GetComponentInParent<MaskTreeUI>();
        hover = transform.Find("Hover").gameObject;
    }


    public void OnSelect(BaseEventData e)
    {
        ui.OnNodeHoverEnter(nodeIndex);
        hover.SetActive(true);
    }

    public void OnDeselect(BaseEventData e)
    {
        ui.OnNodeHoverExit();
        hover.SetActive(false);
    }
}