[GlobalClass]
public partial class Rarity : Resource
{
    [Export] public string id = Guid.NewGuid().ToString();
    [Export] public int Tier { get; set; }
    [Export] public float DropWeight { get; set; }
    [Export] public int MaxAffixes { get; set; }
    [Export] public int MinAffixes { get; set; }
    [Export] public Godot.Collections.Array<LootDefinition> AllowedDrops { get; set; } = new();
    [Export] public bool CanHaveUniqueMods { get; set; }
    [Export] public Color DisplayColor { get; set; }
    [Export] public float LightIntensity { get; set; } = 0.05f;
    [Export] public float LightRange { get; set; } = 0.05f;

    public Guid Id => Guid.TryParse(id, out var g) ? g : Guid.Empty;
}