namespace HeroLimitBreak;

public sealed class ItemInventory
{
    private readonly Dictionary<int, int> _itemCountById = [];

    public int GetCount(int itemId)
    {
        return _itemCountById.GetValueOrDefault(itemId);
    }

    public void AddItem(int itemId, int amount)
    {
        if (itemId <= 0 || amount <= 0)
        {
            return;
        }

        _itemCountById[itemId] = GetCount(itemId) + amount;
    }

    public void UseItems(Dictionary<int, int> requiredItems)
    {
        foreach (var requiredItem in requiredItems)
        {
            UseItem(requiredItem.Key, requiredItem.Value);
        }
    }

    public bool UseItem(int itemId, int amount)
    {
        if (itemId <= 0 || amount <= 0)
        {
            return false;
        }

        var currentCount = GetCount(itemId);
        if (currentCount < amount)
        {
            return false;
        }

        var remainCount = currentCount - amount;
        if (remainCount == 0)
        {
            _itemCountById.Remove(itemId);
        }
        else
        {
            _itemCountById[itemId] = remainCount;
        }

        return true;
    }
}