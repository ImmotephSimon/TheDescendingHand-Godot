[GlobalClass]
public partial class PassiveTreeData : Resource
{
    [Export] public Godot.Collections.Array<PassiveNode> Nodes { get; set; } = new();
    [Export] public Godot.Collections.Array<PassiveNodeType> NodeTypes { get; set; } = new();
    [Export] public Godot.Collections.Array<PassiveNodeConnection> Connections { get; set; } = new();
}

