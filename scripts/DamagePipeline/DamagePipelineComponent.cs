

[GlobalClass]
public partial class DamagePipelineComponent : Node3D
{
    // 1. Calculations & Gates: Strictly ordered, returns bool to cancel
    private static readonly IDamagePipelineStep[] _preProcessSteps = new IDamagePipelineStep[]
    {
        //new InvulnerabilityStep(),
        new DamageImmunityStep(),
        new DamageMitigationStep(),
        //new CriticalHitStep()
    };

    // 2. Reactive Side Effects: Fire-and-forget observers
    private static readonly IDamagePipelineStep[] _postProcessSteps = new IDamagePipelineStep[]
    {
        //new LifeStealStep(),
        //new DamageTextUI()
    };

    public bool RunPreProcess(IEntity owner, DamageInfo info, out float totalDamage)
    {
        for (int i = 0; i < _preProcessSteps.Length; i++)
        {
            if (!_preProcessSteps[i].RunStep(owner, info))
            {
                totalDamage = 0f;
                return false;
            }
        }

        totalDamage = 0f;
        foreach (var amount in info.DamageMap.Values)
            totalDamage += amount;

        return true;
    }

    public void RunPostProcess(IEntity owner, DamageInfo info, float appliedDamage)
    {
        if (appliedDamage <= 0) return; // Guard once at the root level

        for (int i = 0; i < _postProcessSteps.Length; i++)
        {
            _postProcessSteps[i].RunStep(owner, info);
        }
    }
}