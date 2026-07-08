using UnityEngine.EventSystems;
using UnityEngine;

public class MaskButton :
    MonoBehaviour,
    ISelectHandler

{
    public int maskIndex;

    private MaskTreeUI ui;

    [SerializeField] public GameObject hover;

    private void Awake()
    {
        hover = transform.Find("Hover").gameObject;
    }

    public void OnSelect(BaseEventData e)
    {
        ui.SelectMask(maskIndex);
        hover.SetActive(true);
    }

}