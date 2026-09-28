[GlobalClass]
public partial class RarityExpMultiplier : Resource
{
    [Export] public Rarity Rarity { get; set; }
    [Export] public float Multiplier { get; set; } = 1f;
}