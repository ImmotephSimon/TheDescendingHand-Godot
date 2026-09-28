public enum OrbEffect { Corruption, AddPhys, AddFire, AddCold, AddLightning }

[GlobalClass]
public partial class OrbComponentDefinition : ItemComponentDefinition
{
    [Export] public TagContainer TargetRequirements { get; set; }
    [Export] public OrbEffect OrbEffect { get; set; }

    public override ItemUseComponent CreateRuntimeComponent()
    {
        return new OrbUseComponent(this);
    }

    public InventoryResponse ApplyEffect(ItemDropInstance targetItem)
    {
        // TODO: Modify targetItem (e.g., add affix, reroll, etc.)

        //return OrbEffect switch
        //{
        //    OrbEffect.Corruption => affixes.ApplyCorruption() ? InventoryResponse.Consumed : InventoryResponse.Failed,
        //    OrbEffect.AddPhys => affixes.AddAffixTag(DamageType.Physical) ? InventoryResponse.Consumed : InventoryResponse.Failed,
        //    _ => InventoryResponse.Failed
        //};
        return InventoryResponse.Consumed;
    }
}