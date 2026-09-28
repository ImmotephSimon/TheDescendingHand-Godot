[GlobalClass]
public partial class AbilityManager : Node, IAbilityManager
{
    private readonly Dictionary<IEnemyAttack, float> _cooldowns = new();
    private readonly List<IEnemyAttack> _keys = new();

    public bool IsReady(IEnemyAttack ability)
    {
        return !_cooldowns.TryGetValue(ability, out var time) || time <= 0;
    }

    public void StartCooldown(IEnemyAttack ability)
    {
        _cooldowns[ability] = ability.CooldownDuration;
    }

    public override void _Process(double delta)
    {
        if (_cooldowns.Count == 0) return;

        float dt = (float)delta;

        _keys.Clear();
        _keys.AddRange(_cooldowns.Keys);

        for (int i = 0; i < _keys.Count; i++)
        {
            var ability = _keys[i];
            float remaining = _cooldowns[ability] - dt;

            if (remaining <= 0)
                _cooldowns.Remove(ability);
            else
                _cooldowns[ability] = remaining;
        }
    }
}