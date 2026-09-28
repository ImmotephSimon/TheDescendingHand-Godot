[GlobalClass]
public partial class GoldLootDefinition : LootDefinition
{
    public override void Initialize(WorldDrop drop, Rarity rarity)
    {
        int amount = Math.Min(Random.Shared.Next(1, 11), Random.Shared.Next(1, 11));
        if (drop is GoldDrop goldDrop) goldDrop.Initialize(amount);
    }
}