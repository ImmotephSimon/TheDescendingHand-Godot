public class DamageImmunityStep : IDamagePipelineStep
{
    private readonly Dictionary<GameTag, float> _lockoutExpirations = new();
    private readonly List<GameTag> _keysToRemove = new();

    public bool RunStep(IEntity owner, DamageInfo info)
    {
        if (info.DamageMap?.Count == 0) return false;
        float lockout = info.Source.Stats.GetStat(GameTags.ModSpecialDamageTypeLockout);
        if (lockout <= 0f) return true;

        float time = (float)(Time.GetTicksMsec() * 0.001);
        _keysToRemove.Clear();

        foreach (var (type, _) in info.DamageMap)
        {
            if (_lockoutExpirations.TryGetValue(type, out var exp) && time < exp) _keysToRemove.Add(type);
            else _lockoutExpirations[type] = time + lockout;
        }
        foreach (var key in _keysToRemove) 
            info.DamageMap.Remove(key);

        return info.DamageMap.Count > 0;
    }
}