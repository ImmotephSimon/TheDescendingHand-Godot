[GlobalClass]
public partial class DropTableEntry : Resource
{
    [Export] public Rarity Rarity { get; set; }
    [Export] public float Weight { get; set; }
    [Export] public LootDefinition Loot { get; set; }
}