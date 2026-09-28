using Godot;
using System;

[GlobalClass]
public partial class AffixDefinition : Resource
{
    [Export] private string id = "";
    [Export] public string NameOverride { get; set; }
    [Export] public Godot.Collections.Array<StatModifier> Mods { get; set; } = new();
    [Export] public TagRequirement ContextRequirement { get; set; }
    [Export] public Godot.Collections.Array<GameTag> Prerequisites { get; set; } = new();
    [Export] public Godot.Collections.Array<GameTag> Tags { get; set; } = new();
    [Export] public AffixSlot Slot { get; set; }

    public Guid Id => Guid.TryParse(id, out var g) ? g : GenerateAndSaveId();

    private Guid GenerateAndSaveId()
    {
        var newId = Guid.NewGuid();
        id = newId.ToString();
        return newId;
    }
}