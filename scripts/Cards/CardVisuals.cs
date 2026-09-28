[GlobalClass]
public partial class CardVisuals : Resource
{
    [Export] public string Name { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public Texture2D Icon { get; set; }
    [Export] public Animation AnimationOverride { get; set; }
    [Export] public Godot.Collections.Array<PackedScene> Vfx { get; set; } = new();

    public CardVisuals() { }
}