[GlobalClass]
public partial class PlayerInventory : BaseInventory, ITargetable
{
    public override int Rows => 6;
    public override int Columns => 8;

    private Loadout _loadout;
    public Loadout Loadout => _loadout ??= new Loadout(this.GetComponentInOwner<IEntity>());

    public override void _Ready()
    {
        base._Ready();
        Loadout.OnItemUnequipped += item => TryAdd(item);
        Loadout.CanUnequipToDestination = CanAdd;
    }

    public override void SlotRightClicked(int row, int column)
    {
        if (!TryGet(row, column, out IInventoryItem inventoryItem))
            return;

        if (inventoryItem is not ItemDropInstance item)
        {
            GD.PrintErr($"Can't equip {inventoryItem}");
            return;
        }
        if (!item.BaseType.Components.Any(x => x is EquipComponentDefinition))
        {
            GD.Print($"Item cannot be equipped.");
            return;
        }

        if (!Loadout.Equip(item))
        {
            GD.PrintErr($"Loadout returned false.");
            return;
        }
        if (!TryRemove(item))
        {
            GD.PrintErr($"Failed to remove item.");
            return;
        }
    }

    public override void SlotLeftClicked(int row, int column)
    {
        var cursorController = CursorItemController.Instance;
        if (cursorController == null || cursorController.CursorItem == null) return;

        CursorItem cursor = cursorController.CursorItem;

        bool isCtrlPressed = Input.IsKeyPressed(Key.Ctrl);
        if (isCtrlPressed)
        {
            if (TryGet(row, column, out IInventoryItem slotItem))
            {
                DropInWorld(slotItem);
            }
            return;
        }

        if (cursor.HasItem)
        {
            IInventoryItem held = cursor.HeldItem;

            if (TryGet(row, column, out IInventoryItem slotItem))
            {
                if (TryRemove(slotItem))
                {
                    if (TryAddAt(held, row, column))
                    {
                        cursor.Clear();
                        cursor.Hold(slotItem);
                    }
                    else
                    {
                        TryAddAt(slotItem, row, column);
                    }
                }
            }
            else if (TryAddAt(held, row, column))
            {
                cursor.Clear();
            }
            return;
        }

        if (TryGet(row, column, out IInventoryItem itemToPick))
        {
            CursorItemController.Instance.OnDropRequested -= DropInWorld;
            CursorItemController.Instance.OnDropRequested += DropInWorld;
            cursor.Hold(itemToPick);
        }
    }

    private void DropInWorld(IInventoryItem instance)
    {
        ClientBridge.Instance.PlayerNetwork.RequestDrop(instance.InventoryId.ToString());
        CursorItemController.Instance.CursorItem.Clear();
    }

    public TagContainer GetTargetingRequirements()
    {
        throw new NotImplementedException();
    }

    public InventoryResponse ApplyTargetedEffect(ItemDropInstance targetItem)
    {
        throw new NotImplementedException();
    }

    public void StartTargeting(ItemDropInstance orbInstance, TagContainer requirements, Action<ItemDropInstance> onTargetSelected)
    {
        throw new NotImplementedException();
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        if (CursorItemController.Instance != null)
            CursorItemController.Instance.OnDropRequested -= DropInWorld;
    }
}