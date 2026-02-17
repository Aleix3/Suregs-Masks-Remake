using UnityEngine;

public class PlayerEconomy : MonoBehaviour
{
    public int gold = 100;

    public static PlayerEconomy instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject); 
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

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
