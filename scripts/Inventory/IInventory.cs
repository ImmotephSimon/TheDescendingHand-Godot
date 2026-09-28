public interface IInventory
{
    int Rows { get; }
    int Columns { get; }
    bool TryGet(Guid id, out IInventoryItem item);
    IReadOnlyDictionary<IInventoryItem, Vector2I> GetItemPositions();
    bool TryGet(int row, int column, out IInventoryItem item);

    bool TryAdd(IInventoryItem item);
    bool TryRemove(IInventoryItem item);

    void SlotLeftClicked(int row, int column);
    void SlotRightClicked(int row, int column);

    event Action OnChanged;
}