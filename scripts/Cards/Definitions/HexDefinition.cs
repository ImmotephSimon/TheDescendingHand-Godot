[GlobalClass]
public partial class HexDefinition : CardDefinition
{
    [Export] private float _radius = 3f;
    [Export] private float _delay = 2f;
    [Export] private float _curseDuration = 5f;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var overlap = card.AddSphereOverlap(_radius);
        var delayed = card.AddCardComponent<DelayedComponent>();
        delayed.Configure(_delay);
        var statusEffect = card.AddCardComponent<StatusEffectComponent>();
        statusEffect.Configure(GameTags.StatusHexFrailty, _curseDuration);

        card.OnActivated += () => context.ClientSpawn(
            this, 
            new VfxSpawnParams(card.TargetLocation, 0, _delay) { Scale = Vector3.One * _radius }
        );
        delayed.OnCompleted += () => card.GlobalPosition = card.TargetLocation;
    }
}