using UnityEngine;

public class PotionShop : MonoBehaviour, IInteractable
{
    public int potionCost = 25;
    public float healAmount = 30f;

    Player player;
    PlayerEconomy economy;

    void Start()
    {
        player = FindObjectOfType<Player>();
        economy = FindObjectOfType<PlayerEconomy>();
    }

    public void Interact()
    {
    }

    public void BuyPotion()
    {
        if (economy.TrySpendGold(potionCost))
        {
            player.Heal(healAmount);
        }
    }
}
