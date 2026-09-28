[GlobalClass]
public partial class PassiveNodeView : Control
{
    [Export] private TextureRect _image;
    [Export] private CanvasItem _selectionHighlight;
    [Export] private CanvasItem _allocatedHighlight;
    [Export] private Color _allocatedColor = Colors.White;
    [Export] private Color _unallocatedColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    public PassiveNode NodeData { get; private set; }
    public PassiveNodeType NodeType { get; private set; }
    public bool IsAllocated { get; private set; }

    public event Action<PassiveNodeView> HoverEnter;
    public event Action<PassiveNodeView> HoverExit;
    private Action<PassiveNodeView, MouseButton> _onClicked;

    public override void _Ready()
    {
        MouseEntered += () => HoverEnter?.Invoke(this);
        MouseExited += () => HoverExit?.Invoke(this);
    }

    public void SetNode(PassiveNode node, PassiveNodeType type, Action<PassiveNodeView, MouseButton> onClick)
    {
        NodeData = node;
        NodeType = type;
        _onClicked = onClick;
        Position = node.Position - Size * 0.5f;

        if (_image != null && type != null) _image.Texture = type.Texture;
        SetHighlight(false);
        SetAllocated(false);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true } mb)
            _onClicked?.Invoke(this, mb.ButtonIndex);
    }

    public void SetHighlight(bool isSelected) =>
        _selectionHighlight?.Set("visible", isSelected);

    public void SetAllocated(bool isAllocated)
    {
        IsAllocated = isAllocated;
        if (_allocatedHighlight != null) _allocatedHighlight.Visible = isAllocated;
        if (_image != null) _image.Modulate = isAllocated ? _allocatedColor : _unallocatedColor;
    }
}