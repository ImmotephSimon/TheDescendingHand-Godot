[GlobalClass]
public partial class InventorySlotView : Control
{
    [Export] private TextureRect backgroundImage;
    [Export] private RichTextLabel debugText;

    private int _row;
    private int _column;

    public int Row => _row;
    public int Column => _column;

    public event Action<InventorySlotView, InputEventMouseButton> OnSlotRightClicked;
    public event Action<InventorySlotView, InputEventMouseButton> OnSlotClicked;
    public event Action<InventorySlotView> OnSlotHovered;
    public event Action<InventorySlotView> OnSlotUnhovered;

    public override void _Ready()
    {
        MouseEntered += () => OnSlotHovered?.Invoke(this);
        MouseExited += () => OnSlotUnhovered?.Invoke(this);
    }

    public void Initialize(int row, int column)
    {
        _row = row;
        _column = column;
    }

    public void SetHighlight(Color color)
    {
        if (backgroundImage != null)
            backgroundImage.SelfModulate = color;
    }

    public void ClearHighlight()
    {
        if (backgroundImage != null)
            backgroundImage.SelfModulate = Colors.White;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            if (mouseButton.ButtonIndex == MouseButton.Right)
                OnSlotRightClicked?.Invoke(this, mouseButton);
            else if (mouseButton.ButtonIndex == MouseButton.Left)
                OnSlotClicked?.Invoke(this, mouseButton);
        }
    }

}