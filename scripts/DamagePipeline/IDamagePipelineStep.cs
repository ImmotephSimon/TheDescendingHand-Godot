public interface IDamagePipelineStep
{
    bool RunStep(IEntity owner, DamageInfo info);
}