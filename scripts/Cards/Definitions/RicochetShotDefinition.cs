[GlobalClass]
public partial class RicochetShotDefinition : CardDefinition
{
    [Export] private ProjectileInfo _projectileInfo;
    [Export] private float _effectiveness;
    [Export] private GameTag _damageConversion;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(_effectiveness, _damageConversion);

        var projectile = card.AddCardComponent<ProjectileComponent>();
        projectile.Configure(_projectileInfo, NetworkManager.Spawn);
    }
}