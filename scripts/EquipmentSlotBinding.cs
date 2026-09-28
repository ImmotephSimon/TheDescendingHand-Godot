[GlobalClass]
public partial class EquipmentSlotBinding : Resource
{
    [Export] public EquipmentType Type { get; set; }
    [Export] public NodePath BoneAnchorPath { get; set; }
}