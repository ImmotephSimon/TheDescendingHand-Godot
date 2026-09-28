[GlobalClass]
public partial class HexBurstDefinition : CardDefinition
{
    [Export] private float _radius = 3f;
    [Export] private float _effectiveness;
    [Export] private GameTag _damageConversion;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var overlap = card.AddSphereOverlap(_radius);
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(_effectiveness, _damageConversion, triggerOnHit: false);

        card.OnHit += info =>
        {
            var entity = info.Target;
            foreach (GameTag hex in GameTags.Hexes)
            {
                if (entity.Stats.GetStat(hex) > 0f)
                {
                    damage.TriggerDamage(info, _effectiveness);
                    entity.Stats.RemoveModifier(hex);
                    context.ClientSpawn.Invoke(this, new VfxSpawnParams(entity.Node.GlobalPosition));
                }
            }
        };
    }
}