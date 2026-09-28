public abstract class InventoryItemInstance : IInventoryItem
{
    public abstract Guid InventoryId { get; protected set; }
    public abstract Vector2I Size { get; }
    public abstract Texture2D Icon { get; }

    public abstract void Display();
}