[GlobalClass]
public partial class EquipmentVisuals : Node
{
    [Export] private Godot.Collections.Array<EquipmentSlotBinding> slots;

    private readonly Dictionary<EquipmentType, Node3D> _boneAnchors = new();
    private readonly Dictionary<EquipmentType, Node3D> _activeVisuals = new();

    public override void _Ready()
    {
        if (slots == null) { GD.PushError($"EquipmentVisuals '{GetPath()}' has no slots assigned"); return; }

        foreach (var slot in slots)
        {
            if (!slot.BoneAnchorPath.IsEmpty)
                _boneAnchors[slot.Type] = GetNode<Node3D>(slot.BoneAnchorPath);
        }
    }

    public void SetEquipment(EquipmentType type, ItemDefinition item)
    {
        if (item == null) { ClearEquipment(type); return; }
        if (item.Appearance?.WorldModel == null) return;

        ClearEquipment(type);
        if (!_boneAnchors.TryGetValue(type, out var anchor)) return;

        var visual = item.Appearance.WorldModel.Instantiate<Node3D>();
        anchor.AddChild(visual);
        visual.Position = Vector3.Zero;
        visual.Rotation = Vector3.Zero;
        visual.Scale = Vector3.One;

        _activeVisuals[type] = visual;
    }

    public void ClearEquipment(EquipmentType type)
    {
        if (_activeVisuals.TryGetValue(type, out var currentVisual))
        {
            if (GodotObject.IsInstanceValid(currentVisual)) currentVisual.QueueFree();
            _activeVisuals.Remove(type);
        }
    }
}