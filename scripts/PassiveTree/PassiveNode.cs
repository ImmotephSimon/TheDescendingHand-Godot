public enum PassiveNodeRole
{
    Unset,
    Starter,
    Keystone
}

[GlobalClass]
public partial class PassiveNode : Resource
{
    [Export] public string Name { get; set; }
    [Export] public string Type { get; set; }
    [Export] public Vector2 Position { get; set; }
    [Export] public PassiveNodeRole Role { get; set; }
    [Export] public Godot.Collections.Array<PassiveNodeCost> Cost { get; set; } = new();
    [Export] public string Cluster { get; set; }
    [Export] public Godot.Collections.Array<PassiveNodeCost> Requirement { get; set; } = new();
}