[GlobalClass]
public partial class CursorItem : Control
{
    [Export] private ItemIconView icon;

    public IInventoryItem HeldItem { get; private set; }
    public bool HasItem => HeldItem != null;

    public override void _Process(double delta)
    {
        if (HasItem)
            UpdatePosition();
    }

    public void Hold(IInventoryItem item)
    {
        HeldItem = item;
        Visible = true;

        Vector2I gridDimensions = item.Size;
        Vector2 pixelSize = new Vector2(gridDimensions.X * 64f, gridDimensions.Y * 64f);

        icon.Render(item.Icon, Vector2.Zero, pixelSize);
        UpdatePosition();
    }

    public IInventoryItem Clear()
    {
        IInventoryItem item = HeldItem;
        HeldItem = null;
        Visible = false;
        return item;
    }

    private void UpdatePosition()
    {
        GlobalPosition = GetViewport().GetMousePosition();
    }
}