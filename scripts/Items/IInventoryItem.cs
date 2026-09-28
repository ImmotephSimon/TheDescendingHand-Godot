public interface IInventoryItem
{
    Guid InventoryId { get; }
    Vector2I Size { get; }
    Texture2D Icon { get; }

    void Display();
}