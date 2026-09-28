[GlobalClass]
public partial class ItemLootDefinition : LootDefinition
{
	public override float DropHeight { get; set; } = 1f;
	public override float UpForce { get; set; } = 2f;
	public override float ForwardForce { get; set; } = 0.5f;
	public override float Torque { get; set; } = 1.5f;

	public override void Initialize(WorldDrop drop, Rarity rarity)
	{
		if (drop is ItemDrop itemDrop)
		{
			var item = GameData.Instance.Items.RollRandomItem();
			GD.Print($"ItemDrop rolled: {item.DisplayName}");
			itemDrop.Initialize(item, rarity);
		}
	}
}