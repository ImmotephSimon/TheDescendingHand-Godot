[GlobalClass]
public partial class InventoryView : Control
{
    [Export] private Control panelRoot;
    [Export] private PackedScene slotPrefab;
    [Export] private PackedScene itemIconPrefab;
    [Export] private Node itemContainer;
    [Export] private Vector2 cellSize = new(64f, 64f);

    private readonly Vector2 spacing = new(2f, 2f);
    private readonly List<ItemIconView> spawnedIcons = new();

    internal void ToggleVisibility() => SetVisibility(!panelRoot.Visible);

    private PlayerItemsSync _items;
    private InventorySlotView[,] slots;

    public void Bind(PlayerItemsSync items)
    {
        if (_items != null)
            _items.InventoryChanged -= Refresh;

        _items = items;

        if (_items == null)
            return;

        _items.InventoryChanged += Refresh;
    }

    public void Initialize()
    {
        InitializeGrid();
        Refresh();
        SetVisibility(false);
    }

    public override void _ExitTree()
    {
        if (_items != null)
            _items.InventoryChanged -= Refresh;
    }

    private void InitializeGrid()
    {
        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot != null && GodotObject.IsInstanceValid(slot))
                    slot.QueueFree();
            }
        }

        int rows = _items.InventoryRows;
        int columns = _items.InventoryColumns;

        if (panelRoot is GridContainer gridLayout)
        {
            gridLayout.Columns = columns;
            gridLayout.AddThemeConstantOverride("h_separation", (int)spacing.X);
            gridLayout.AddThemeConstantOverride("v_separation", (int)spacing.Y);
        }

        slots = new InventorySlotView[rows, columns];

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                var slot = slotPrefab.Instantiate<InventorySlotView>();
                AddChild(slot);
                slot.Initialize(row, column);

                slot.OnSlotClicked += HandleSlotClicked;
                slot.OnSlotRightClicked += HandleSlotRightClicked;
                slot.OnSlotHovered += HandleSlotHovered;
                slot.OnSlotUnhovered += HandleSlotUnhovered;

                slots[row, column] = slot;
            }
        }
    }

    private void HandleSlotClicked(InventorySlotView slot, InputEventMouseButton eventData)
    {
        _items.RequestInventorySlotClick(slot.Row, slot.Column);
    }

    private void HandleSlotRightClicked(InventorySlotView slot, InputEventMouseButton eventData)
    {
        _items.RequestInventorySlotRightClick(slot.Row, slot.Column);
    }

    private void HandleSlotHovered(InventorySlotView slot)
    {
        _items.Server_RequestInventoryTooltip(slot.Row, slot.Column);
    }

    private void HandleSlotUnhovered(InventorySlotView slot)
    {
        TooltipController.Instance.HideTooltip();
    }

    private void Refresh()
    {
        foreach (var icon in spawnedIcons)
        {
            if (icon != null && GodotObject.IsInstanceValid(icon))
                icon.QueueFree();
        }

        spawnedIcons.Clear();

        Node parent = itemContainer != null ? itemContainer : this;

        foreach (var entry in _items.InventoryItems)
        {
            if (!GameData.Instance.Items.TryGetIcon(entry.ItemId, out var icon))
            {
                continue;
            }
            Vector2I origin = entry.Position;

            ItemIconView iconView = itemIconPrefab.Instantiate<ItemIconView>();
            parent.AddChild(iconView);
            spawnedIcons.Add(iconView);

            Vector2 position = GetLocalPosition(origin.X, origin.Y);
            Vector2I size = entry.Size;

            Vector2 pixelSize = new(
                size.X * cellSize.X + (size.X - 1) * spacing.X,
                size.Y * cellSize.Y + (size.Y - 1) * spacing.Y
            );

            iconView.Render(icon, position, pixelSize);
        }
    }

    private Vector2 GetLocalPosition(int column, int row)
    {
        float x = column * (cellSize.X + spacing.X);
        float y = -row * (cellSize.Y + spacing.Y);

        return new Vector2(x, y);
    }

    public void SetVisibility(bool visible)
    {
        if (panelRoot != null)
            panelRoot.Visible = visible;

        if (!visible)
            TooltipController.Instance.HideTooltip();
    }
}