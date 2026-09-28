[GlobalClass]
public partial class CardItemInstance : RefCounted, IInventoryItem
{
    public Guid InstanceId { get; } = Guid.NewGuid();

    public CardDefinition Definition { get; }

    public Vector2I Size => new Vector2I(1, 1);

    public Texture2D Icon => Definition.Visuals.Icon;

    // Optional: Components list if cards share right-click/use logic with items
    public List<ItemUseComponent> Components { get; } = new();

    public Guid InventoryId => Definition.Id;

    public CardItemInstance(CardDefinition definition)
    {
        Definition = definition;
    }

    public void Use(IEntity owner)
    {
        // Execute card learning, deck addition, or casting logic here
    }

    public void Display()
    {
        throw new NotImplementedException();
    }
}