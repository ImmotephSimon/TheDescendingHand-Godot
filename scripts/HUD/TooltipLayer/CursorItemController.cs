using System;
using Godot;

[GlobalClass]
public partial class CursorItemController : Control
{
    [Export] private PackedScene prefab;
    [Export] private Control tooltipLayer;

    public static CursorItemController Instance { get; private set; }
    public event Action<IInventoryItem> OnDropRequested;
    public CursorItem CursorItem { get; private set; }

    public override void _Ready()
    {
        Instance = this;
        CursorItem = prefab.Instantiate<CursorItem>();
        tooltipLayer.AddChild(CursorItem);
        CursorItem.Visible = false;
    }

    public void TryDropHeldItem()
    {
        if (CursorItem == null || !CursorItem.HasItem)
            return;

        if (GetViewport().GuiGetHoveredControl() == null)
            OnDropRequested?.Invoke(CursorItem.HeldItem);
    }
}