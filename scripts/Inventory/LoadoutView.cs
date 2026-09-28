[GlobalClass]
public partial class LoadoutView : Control
{
    [Export] private Godot.Collections.Array<LoadoutSlotView> slots = new();
    [Export] private Texture2D emptySlotTexture;

    private PlayerItemsSync _items;

    public override void _Ready()
    {
        foreach (var slot in slots)
        {
            slot.OnSlotRightClicked += HandleSlotRightClicked;
            slot.OnSlotHoverEnter += HandleSlotHoverEnter;
            slot.OnSlotHoverExit += HandleSlotHoverExit;
        }
    }

    private void HandleSlotHoverEnter(EquipmentType type)
    {
        if (_items == null) return;
        var dto = _items.EquippedItems.FirstOrDefault(x => x.EquipmentTypeId == type.Id);
        if (dto.Equals(default(ItemTooltipDto))) return;

        TooltipController.Instance?.ShowItem(dto);
    }

    private void HandleSlotHoverExit()
    {
        TooltipController.Instance?.HideTooltip();
    }

    private void HandleSlotRightClicked(EquipmentType type)
    {
        _items?.Server_RequestUnequip(type.Id.ToString());
    }

    public void Bind(PlayerItemsSync items)
    {
        if (_items != null)
            _items.LoadoutChanged -= RefreshLoadoutView;

        _items = items;

        if (_items == null)
        {
            ClearView();
            return;
        }

        _items.LoadoutChanged += RefreshLoadoutView;
        RefreshLoadoutView();
    }

    public override void _ExitTree()
    {
        if (_items != null)
            _items.LoadoutChanged -= RefreshLoadoutView;
    }

    private void RefreshLoadoutView()
    {
        if (_items == null) return;

        foreach (var slot in slots)
        {
            var dto = _items.EquippedItems
                .FirstOrDefault(x => x.EquipmentTypeId == slot.SlotType.Id);

            if (dto.Equals(default(ItemTooltipDto)))
            {
                slot.UpdateSlot(null, emptySlotTexture);
                continue;
            }

            if (GameData.Instance.Items.TryGetDefinition(dto.BaseTypeId, out ItemDefinition def))
            {
                slot.UpdateSlot(def.Appearance.Icon, emptySlotTexture);
            }
        }
    }

    private void ClearView()
    {
        foreach (var slot in slots)
            slot.UpdateSlot(null, emptySlotTexture);
    }
}