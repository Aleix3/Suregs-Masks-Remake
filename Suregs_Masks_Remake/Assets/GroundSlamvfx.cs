using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundSlamvfx : MonoBehaviour
{
    public void DestroyVFX()
    {
        Destroy(transform.parent.gameObject);
    }
}
