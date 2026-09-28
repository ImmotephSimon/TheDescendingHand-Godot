[GlobalClass]
public partial class PassiveNodeType : Resource
{
    [Export] public string Name { get; set; }
    [Export] public Godot.Collections.Array<StatModifier> Modifiers { get; set; } = new();
    [Export] public Texture2D Texture { get; set; }
    [Export] public string DescriptionOverride { get; set; }
}