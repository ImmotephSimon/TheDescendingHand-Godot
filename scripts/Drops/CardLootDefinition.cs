[GlobalClass]
public partial class CardLootDefinition : LootDefinition
{
    public override void Initialize(WorldDrop drop, Rarity rarity)
    {
        if (drop is CardDrop cardDrop)
            cardDrop.Initialize(GameData.Instance.Cards.GetRandomCard());
    }
}