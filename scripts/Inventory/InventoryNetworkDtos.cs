public struct InventoryItemDto
{
    public Guid ItemId;
    public Vector2I Position;
    public Vector2I Size;
}

public struct CardTooltipDto
{
    public string CardId;
    // Add any dynamic card state (e.g., Level, Foil, XP)
}

public struct ItemTooltipDto
{
    public Guid EquipmentTypeId;
    public Guid BaseTypeId;
    public Guid RarityId;
    public List<AffixState> Implicits;
    public List<AffixState> Explicits;
}

public struct AffixState
{
    public string DisplayName;
    public float Tier;
    public List<StatModifier> ResolvedMods;
    public TagRequirement TagRequirement;

    public static AffixState FromInstance(AffixInstance instance, List<ItemImplicit> itemImplicits)
    {
        return new AffixState
        {
            DisplayName = instance.Definition.NameOverride ?? string.Empty,
            Tier = instance.Tier,
            ResolvedMods = instance.ToStatModifiers(itemImplicits),
            TagRequirement = instance.Definition.ContextRequirement
        };
    }

    public static AffixState FromModifier(StatModifier modifier)
    {
        return new AffixState
        {
            DisplayName = string.Empty,
            Tier = 1f,
            ResolvedMods = new List<StatModifier> { modifier },
            TagRequirement = modifier.RequiredTags
        };
    }
}