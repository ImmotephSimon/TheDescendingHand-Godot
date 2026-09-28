public enum AffixSlot
{
    Prefix,
    Suffix,
    Implicit
}

[GlobalClass]
public partial class ModifierPoolEntry : Resource
{
    [Export] public AffixDefinition Definition { get; set; }
    [Export] public float Weight { get; set; } = 100f;

    public ModifierPoolEntry() { }

    public ModifierPoolEntry(AffixDefinition definition, float weight = 100f)
    {
        Definition = definition;
        Weight = weight;
    }
}