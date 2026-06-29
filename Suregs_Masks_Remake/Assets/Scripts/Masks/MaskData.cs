using UnityEngine;


[CreateAssetMenu(fileName = "MaskData", menuName = "Masks/MaskData")]
public class MaskData : ScriptableObject
{
    public string maskName;
    public int maskID;
    public Sprite maskIcon;
}