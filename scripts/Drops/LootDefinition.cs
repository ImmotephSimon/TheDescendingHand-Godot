public abstract partial class LootDefinition : Resource
{
    [Export] public virtual float DropHeight { get; set; }
    [Export] public virtual float UpForce { get; set; }
    [Export] public virtual float ForwardForce { get; set; }
    [Export] public virtual float Torque { get; set; }

    [Export] public PackedScene Prefab;

    public abstract void Initialize(WorldDrop drop, Rarity rarity);
}