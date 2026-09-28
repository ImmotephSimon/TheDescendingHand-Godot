[GlobalClass]
public partial class ElectrifyDefinition : CardDefinition
{
    [Export] private ProjectileInfo projectileInfo;
    [Export] private float effectiveness;
    [Export] private GameTag damageConversion;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(effectiveness, damageConversion);

        var projectile = card.AddCardComponent<ProjectileComponent>();
        projectile.Configure(projectileInfo, NetworkManager.Spawn);

        var statusEffect = card.AddCardComponent<StatusEffectComponent>();
        statusEffect.Configure(GameTags.StatusElectrified, 4f);
    }
}