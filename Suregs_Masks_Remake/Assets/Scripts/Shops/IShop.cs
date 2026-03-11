public interface IShop
{
    event System.Action OnTradeUpdated;

    int GetPending(Item.ItemType type);

    bool TryGetGoldValue(Item.ItemType type, out int value);
}