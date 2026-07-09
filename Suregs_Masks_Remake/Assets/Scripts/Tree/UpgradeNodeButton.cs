using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeNodeButton :MonoBehaviour, ISelectHandler,
    IDeselectHandler, ISubmitHandler

{
    public int nodeIndex;

    private MaskTreeUI ui;

    [HideInInspector] public GameObject hover;

    void Awake()
    {
        ui = FindAnyObjectByType<MaskTreeUI>();
        hover = transform.Find("Hover").gameObject;
    }


    public void OnSelect(BaseEventData e)
    {
        ui.OnNodeSelected(nodeIndex);
        hover.SetActive(true);
    }

    public void OnDeselect(BaseEventData e)
    {
        hover.SetActive(false);
    }

    public void OnSubmit(BaseEventData e) => ui.BuyUpgrade(nodeIndex);
}