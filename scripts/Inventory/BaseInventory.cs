using System;
using System.Collections.Generic;
using Godot;

[GlobalClass]
public partial class BaseInventory : Node3D, IInventory
{
    public virtual int Rows => 4;
    public virtual int Columns => 4;

    private IInventoryItem[,] occupancyGrid;
    private readonly Dictionary<IInventoryItem, Vector2I> itemPositions = new();

    public event Action OnChanged;

    protected virtual void Awake() => occupancyGrid = new IInventoryItem[Rows, Columns];

    public IReadOnlyDictionary<IInventoryItem, Vector2I> GetItemPositions() => itemPositions;

    public bool TryGet(int row, int column, out IInventoryItem item)
    {
        if (!IsWithinGrid(row, column)) { item = null; return false; }
        item = occupancyGrid[row, column];
        return item != null;
    }

    public bool TryAdd(IInventoryItem item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        if (itemPositions.ContainsKey(item))
        {
            GD.PrintErr($"[{nameof(TryAdd)}] Item '{item}' is already present in inventory.");
            return false;
        }

        if (!TryFindFreeSlot(item.Size, out Vector2I origin)) return false;

        OccupyCells(item, origin);
        OnChanged?.Invoke();
        return true;
    }

    public bool TryAddAt(IInventoryItem item, int row, int column)
    {
        Vector2I size = item.Size;
        if (row < 0 || column < 0 || row + size.Y > Rows || column + size.X > Columns) return false;
        if (!CanFitAt(row, column, size)) return false;

        OccupyCells(item, new Vector2I(column, row));
        OnChanged?.Invoke();
        return true;
    }

    public bool TryRemove(IInventoryItem item)
    {
        if (!itemPositions.TryGetValue(item, out Vector2I origin)) return false;

        ClearCells(item, origin);
        itemPositions.Remove(item);
        OnChanged?.Invoke();
        return true;
    }

    public bool CanAdd(Vector2I itemSize) => TryFindFreeSlot(itemSize, out _);

    private bool TryFindFreeSlot(Vector2I size, out Vector2I origin)
    {
        for (int r = 0; r <= Rows - size.Y; r++)
        {
            for (int c = 0; c <= Columns - size.X; c++)
            {
                if (CanFitAt(r, c, size))
                {
                    origin = new Vector2I(c, r);
                    return true;
                }
            }
        }
        origin = Vector2I.Zero;
        return false;
    }

    private bool CanFitAt(int startRow, int startCol, Vector2I size)
    {
        for (int r = 0; r < size.Y; r++)
        {
            for (int c = 0; c < size.X; c++)
            {
                if (occupancyGrid[startRow + r, startCol + c] != null) return false;
            }
        }
        return true;
    }

    private void OccupyCells(IInventoryItem item, Vector2I origin)
    {
        Vector2I size = item.Size;
        for (int r = 0; r < size.Y; r++)
        {
            for (int c = 0; c < size.X; c++)
            {
                occupancyGrid[origin.Y + r, origin.X + c] = item;
            }
        }
        itemPositions[item] = origin;
    }

    private void ClearCells(IInventoryItem item, Vector2I origin)
    {
        Vector2I size = item.Size;
        for (int r = 0; r < size.Y; r++)
        {
            for (int c = 0; c < size.X; c++)
            {
                occupancyGrid[origin.Y + r, origin.X + c] = null;
            }
        }
    }

    private bool IsWithinGrid(int r, int c) => r >= 0 && r < Rows && c >= 0 && c < Columns;

    public virtual void SlotLeftClicked(int row, int column) { }
    public virtual void SlotRightClicked(int row, int column) { }

    public bool TryGet(Guid id, out IInventoryItem item)
    {
        foreach (var entry in itemPositions)
        {
            if (entry.Key.InventoryId == id)
            {
                item = entry.Key;
                return true;
            }
        }
        item = null;
        return false;
    }
}