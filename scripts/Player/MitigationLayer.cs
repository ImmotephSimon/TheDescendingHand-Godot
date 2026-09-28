
public class DamageMitigationStep : IDamagePipelineStep
{
    public bool RunStep(IEntity owner, DamageInfo info)
    {
        var node = owner as Godot.Node;
        var health = node?.GetComponentInOwner<IHealth>();
        var stats = node?.GetComponentInOwner<IStatContainer>();

        Debug.Assert(health != null, $"[DamageMitigationStep] Missing IHealth on '{node?.Name}'");
        Debug.Assert(stats != null, $"[DamageMitigationStep] Missing IStatContainer on '{node?.Name}'");

        float maxLife = health.MaxHealth;
        var keys = new List<GameTag>(info.DamageMap.Keys);

        foreach (var type in keys)
        {
            float mitigation = stats.GetStat(GameTags.ModDefenseMitigation, new TagContainer(type));
            info.DamageMap[type] *= maxLife / (maxLife + Godot.Mathf.Max(0f, mitigation));
        }

        return true;
    }
}