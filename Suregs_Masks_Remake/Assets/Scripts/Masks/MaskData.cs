using UnityEngine;


[CreateAssetMenu(fileName = "MaskData", menuName = "Masks/MaskData")]
public class MaskData : ScriptableObject
{
    public string maskName;
    public int maskID;
    public Sprite maskIcon;

    [TextArea] public string abilityDescription;
    [TextArea] public string passiveDescription;
}