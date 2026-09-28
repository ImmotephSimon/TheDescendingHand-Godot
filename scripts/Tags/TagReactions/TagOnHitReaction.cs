[GlobalClass]
public partial class TagOnHitReaction : ReactionType
{
    [Export] public GameTag ListeningTag;
    protected override GameTag GetListeningTag() => ListeningTag;

    [Export] private StatModifier onHitModifier;
    [Export] private float onHitDuration;

    protected override ModifierHandle Apply(IEntity owner, float newValue)
    {
        owner.OnHitStats.Add((onHitModifier, onHitDuration));
        return null;
    }
}