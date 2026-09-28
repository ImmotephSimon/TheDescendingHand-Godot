[GlobalClass]
public partial class FrailtyReaction : ReactionType
{
    [Export] public GameTag ListeningTag { get; set; }

    protected override GameTag GetListeningTag() => ListeningTag;

    protected override ModifierHandle Apply(IEntity owner, float newValue)
    {
        return owner.Stats.AddModifier(new StatModifier(GameTags.ModDefenseDamageTaken, MathOp.Multiplicative, 1.2f));
    }

    
}