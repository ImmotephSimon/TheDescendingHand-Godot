[GlobalClass]
public partial class LoadoutSlotView : Control
{
    [Export] private EquipmentType slotType;
    [Export] private TextureRect icon;

    public event Action<EquipmentType> OnSlotRightClicked;
    public event Action<EquipmentType> OnSlotHoverEnter;
    public event Action OnSlotHoverExit;

    public EquipmentType SlotType => slotType;

    public override void _Ready()
    {
        MouseEntered += () => OnSlotHoverEnter?.Invoke(slotType);
        MouseExited += () => OnSlotHoverExit?.Invoke();
    }

    public void UpdateSlot(Texture2D itemIcon, Texture2D emptySprite)
    {
        var tex = itemIcon != null ? itemIcon : emptySprite;
        icon.Texture = tex;
        icon.Visible = tex != null;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Right)
        {
            OnSlotRightClicked?.Invoke(slotType);
        }
    }
}