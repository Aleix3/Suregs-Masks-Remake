public interface IShop
{
    event System.Action OnTradeUpdated;

    int GetPending(Item.ItemType type);
    int GetRequiredItemQty(Item.ItemType type);

    int GetRequiredItemPending(Item.ItemType type);

    bool TryGetGoldValue(Item.ItemType type, out int value);

    bool IsSelling();
}