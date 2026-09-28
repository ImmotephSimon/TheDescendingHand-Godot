[GlobalClass]
public partial class OrbUseComponent : ItemUseComponent, IUsable
{
    public OrbComponentDefinition Definition { get; }

    public OrbUseComponent(OrbComponentDefinition definition) => Definition = definition;

    public override void Use(ItemDropInstance instance, IEntity user)
    {
        var targeter = user.Node.GetComponentInOwner<ITargetable>();
        targeter?.StartTargeting(instance, Definition.TargetRequirements, targetItem =>
        {
            Definition.ApplyEffect(targetItem);
        });
    }
}