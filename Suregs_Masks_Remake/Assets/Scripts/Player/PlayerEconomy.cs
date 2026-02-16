using UnityEngine;

public class PlayerEconomy : MonoBehaviour
{
    public int gold = 100;

    public bool TrySpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            return true;
        }

        return false;
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }
}
