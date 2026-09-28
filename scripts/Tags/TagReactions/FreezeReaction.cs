[GlobalClass]
public partial class FreezeReaction : ReactionType
{
    [Export] public GameTag ListeningTag { get; set; }
    protected override GameTag GetListeningTag() => ListeningTag;

    protected override ModifierHandle Apply(IEntity owner, float newValue)
    {
        return owner.Stats.AddModifier(new StatModifier(GameTags.ModStatMovement, MathOp.Set, 0));
    }
}