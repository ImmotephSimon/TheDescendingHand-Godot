[GlobalClass]
public partial class ModifierPool : Resource
{
    [Export] public Godot.Collections.Array<ModifierPoolEntry> Entries { get; set; } = new();
}