using UnityEngine;

public class Blacksmith : MonoBehaviour, IInteractable
{
    public int swordUpgradeCost = 50;
    public int armorUpgradeCost = 50;

    public int swordUpgradeAmount = 10;
    public float armorUpgradeAmount = 20f;

    private Player player;
    private PlayerEconomy economy;

    void Start()
    {
        player = FindObjectOfType<Player>();
        economy = FindObjectOfType<PlayerEconomy>();
    }

    public void Interact()
    {

    }

    public void UpgradeSword()
    {
        if (economy.TrySpendGold(swordUpgradeCost))
        {
            player.UpgradeSword(swordUpgradeAmount);
        }
    }

    public void UpgradeArmor()
    {
        if (economy.TrySpendGold(armorUpgradeCost))
        {
            player.UpgradeArmor(armorUpgradeAmount);
        }
    }
}
