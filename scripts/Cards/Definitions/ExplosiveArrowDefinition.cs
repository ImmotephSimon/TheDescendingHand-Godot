using Godot;

[GlobalClass]
public partial class ExplosiveArrowDefinition : CardDefinition
{
    [Export] private ProjectileInfo projectileInfo;
    [Export] private float effectiveness;
    [Export] private GameTag damageConversion;
    [Export] private float detonationDelay;
    [Export] private float channelTickInterval;
    [Export] private float maxChannelDuration;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(
            effectiveness,
            damageConversion);

        var projectile = card.AddCardComponent<ProjectileComponent>();
        projectile.Configure(
            projectileInfo,
            NetworkManager.Spawn);

        var channel = card.AddCardComponent<ChannelingComponent>();
        channel.Configure(
            channelTickInterval,
            maxChannelDuration);

        var delay = card.AddCardComponent<DelayedComponent>();
        delay.Configure(detonationDelay);

        var damageAcc = card.AddCardComponent<DamageAccComponent>();
        damageAcc.Configure(effectiveness);

        // Wire events
        channel.OnTick += projectile.Activate;

        damageAcc.OnReleased += (hits, scalar) =>
        {
            foreach (var hit in hits)
            {
                damage.TriggerDamage(hit, scalar);

                // Only spawns VFX on hit
                context.ClientSpawn(
                    this,
                    new VfxSpawnParams(hit.Position));
            }
        };

        delay.OnCompleted += () =>
        {
            damageAcc.Release();
        };
    }
}