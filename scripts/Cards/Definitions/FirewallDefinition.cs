[GlobalClass]
public partial class FirewallDefinition : CardDefinition
{
    [Export] private PackedScene firewallPrefab;
    [Export] private float effectiveness;
    [Export] private GameTag damageConversion;
    [Export] private float duration = 5f;
    [Export] private float size = 1f;
    [Export] private Vector3 spawnOffset;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var spawn = card.AddCardComponent<MeshSpawnComponent>();
        spawn.Configure(
            firewallPrefab,
            spawnOffset,
            useGravity: false);

        var overlap = card.AddBoxOverlap(
            new Vector3(4f, 2f, 1f) * size);

        var areaDegen = card.AddCardComponent<AreaDegenComponent>();
        areaDegen.Configure(
            overlap,
            damageConversion,
            effectiveness,
            duration,
            1);

        card.OnActivated += () =>
        {
            context.ClientSpawn.Invoke(this, new VfxSpawnParams(card.TargetLocation, 0, duration));
        };
    }
}