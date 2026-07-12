using UnityEngine;

public class MaskPointsOnKill : MonoBehaviour
{
    public int expPointsOnKill = 20;

    public void GrantPoints()
    {
        var mm = Player.Instance?.MaskManager;
        if (mm == null || MaskTreeManager.Instance == null) return;

        // Dar puntos a ambas máscaras equipadas
        if (mm.Primary != null)
        {
            int idx = GetMaskIndex(mm.Primary);
            if (idx >= 0) MaskTreeManager.Instance.AddExpPoints(idx, expPointsOnKill);
        }

        if (mm.Secondary != null)
        {
            int idx = GetMaskIndex(mm.Secondary);
            if (idx >= 0) MaskTreeManager.Instance.AddExpPoints(idx, expPointsOnKill);
        }
    }

    private int GetMaskIndex(BaseMask mask)
    {
        if (mask == null || MaskTreeManager.Instance == null) return -1;
        var masks = MaskTreeManager.Instance.masks;
        for (int i = 0; i < masks.Length; i++)
            if (masks[i] == mask) return i;
        return -1;
    }
}
