public interface IShop
{
    System.Action OnTradeUpdated { get; }

    int GetPending(Item.ItemType type);

    bool TryGetGoldValue(Item.ItemType type, out int value);
}