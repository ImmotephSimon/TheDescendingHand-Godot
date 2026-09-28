[GlobalClass]
public partial class ExperienceTable : Resource
{
    [Export] public Godot.Collections.Array<RarityExpMultiplier> Entries { get; set; } = new();

    public int ScaleByRarity(int baseExperience, Rarity rarity)
    {
        var entry = Entries.FirstOrDefault(x => x.Rarity == rarity);
        return entry == null ? baseExperience : Mathf.RoundToInt(baseExperience * entry.Multiplier);
    }
}

