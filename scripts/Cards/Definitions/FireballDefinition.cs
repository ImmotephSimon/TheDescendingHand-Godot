[GlobalClass]
public partial class FireballDefinition : CardDefinition
{
    [Export] private ProjectileInfo _projectileInfo;
    [Export] private float _effectiveness;
    [Export] private GameTag _damageConversion;
    [Export] private float _duration;
    [Export] private float _radius;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var projectile = card.AddCardComponent<ProjectileComponent>();
        projectile.Configure(_projectileInfo, NetworkManager.Spawn);

        var overlap = card.AddSphereOverlap(_radius);

        var areaDegen = card.AddCardComponent<AreaDegenComponent>();
        areaDegen.Configure(overlap, _damageConversion, _effectiveness, _duration, 1);

        projectile.OnSpawned += controller =>
        {
            areaDegen.Overlap.Attach(controller);
            controller.OnDespawn += areaDegen.Cancel;
        };
    }
}