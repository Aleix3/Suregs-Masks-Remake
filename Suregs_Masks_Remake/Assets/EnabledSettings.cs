using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnabledSettings : MonoBehaviour
{
    public ToggleSpriteSwap[] toggles;
    public void Enable()
    {
        toggles[0].UpdateVisual();
        toggles[1].UpdateVisual();
    }
}
