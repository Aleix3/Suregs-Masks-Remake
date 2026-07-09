using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Experimental.GraphView;

public class MaskButton :
    MonoBehaviour,
    ISelectHandler, IDeselectHandler, ISubmitHandler

{
    public int maskIndex;

    private MaskTreeUI ui;
    private MaskTreeManager tm;
    private Selectable selectable;

    [HideInInspector] public GameObject hover;

    private void Awake()
    {
        ui =  FindAnyObjectByType<MaskTreeUI>();
        tm = MaskTreeManager.Instance;
        selectable = GetComponent<Selectable>();
        hover = transform.Find("Hover").gameObject;
    }

    private void OnEnable() => RefreshInteractable();

    public void RefreshInteractable()
    {
        bool unlocked = tm?.masks[maskIndex]?.data?.isUnlocked ?? false;
        if (selectable != null)
            selectable.interactable = unlocked;
    }

    public void OnSelect(BaseEventData e)
    {
        ui.OnMaskSelected(maskIndex);
        hover.SetActive(true);
    }

    public void OnDeselect(BaseEventData e)
    {
        hover.SetActive(false);
    }

    public void OnSubmit(BaseEventData e) => ui.ShowMask(maskIndex);

}