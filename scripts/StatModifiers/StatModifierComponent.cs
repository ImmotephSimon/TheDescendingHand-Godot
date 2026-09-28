[GlobalClass]
public partial class StatModifierComponent : Node3D, IStatContainer, ICalculator
{
    private readonly Dictionary<GameTag, Dictionary<int, StatModifier>> modifiers = new();
    private readonly Dictionary<GameTag, Action<float>> listeners = new();
    private int nextId = 0;

    public void Listen(GameTag stat, Action<float> callback)
    {
        if (stat == null || callback == null) return;
        if (!listeners.ContainsKey(stat)) listeners[stat] = null;
        listeners[stat] += callback;
    }

    public void StopListening(GameTag stat, Action<float> callback)
    {
        if (stat == null || callback == null) return;
        if (listeners.ContainsKey(stat))
        {
            listeners[stat] -= callback;
            if (listeners[stat] == null) listeners.Remove(stat);
        }
    }

    public void Listen(GameTag[] stats, Action<float> callback)
    {
        foreach (var stat in stats) Listen(stat, callback);
    }

    public void StopListening(GameTag[] stats, Action<float> callback)
    {
        foreach (var stat in stats) StopListening(stat, callback);
    }

    private void NotifyStatChanged(GameTag stat)
    {
        if (stat == null) return;
        if (listeners.TryGetValue(stat, out var callback) && callback != null)
        {
            callback.Invoke(GetStat(stat));
        }
    }

    public ModifierHandle AddModifier(StatModifier modifier, float duration = 0)
    {
        if (modifier.Stat == null)
        {
            GD.PrintErr("Tried to add a null StatModifier.");
            return default;
        }

        int id = nextId++;
        if (!modifiers.TryGetValue(modifier.Stat, out var statModifiers))
        {
            statModifiers = new Dictionary<int, StatModifier>();
            modifiers[modifier.Stat] = statModifiers;
        }

        statModifiers.Add(id, modifier);
        NotifyStatChanged(modifier.Stat);
        ModifierHandle handle = new(id, modifier.Stat);

        if (duration > 0)
            RemoveModifierAfterAsync(handle, duration);

        return handle;
    }

    private async void RemoveModifierAfterAsync(ModifierHandle handle, float duration)
    {
        await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
        RemoveModifier(handle, true);
    }

    public void RemoveModifier(GameTag stat)
    {
        if (!modifiers.ContainsKey(stat))
        {
            GD.PrintErr($"No modifiers found for stat {stat}.");
            return;
        }
        modifiers.Remove(stat);
        NotifyStatChanged(stat);
    }

    public void RemoveModifier(ModifierHandle handle, bool allowPrematureRemoval = false)
    {
        var stat = handle.Stat;
        if (!modifiers.TryGetValue(stat, out var statModifiers))
        {
            if (allowPrematureRemoval) { handle.Invalidate(); return; }
            GD.PrintErr($"No modifiers found for stat {stat}.");
            return;
        }

        if (!statModifiers.Remove(handle.Id))
        {
            if (allowPrematureRemoval) { handle.Invalidate(); return; }
            GD.PrintErr($"Attempted to remove unknown modifier handle {handle.Id}.");
            return;
        }

        if (statModifiers.Count == 0) modifiers.Remove(stat);
        NotifyStatChanged(stat);
        handle.Invalidate();
    }

    public float GetStat(GameTag stat, TagContainer context, float baseValue = 0)
    {
        if (stat == null) return 0;
        var (added, additive, multiplicative) = GetScaling(stat, context);
        return (baseValue + added) * (1 + additive) * multiplicative;
    }

    public float GetStat(GameTag stat) => GetStat(stat, TagContainer.Empty, 0);

    public Dictionary<GameTag, float> CalculateDamage(TagContainer tags, float effectiveness, GameTag damageConversion)
    {
        Dictionary<GameTag, float> _damage = new();
        foreach (GameTag damageType in GameTags.DamageTypes)
        {
            var baseDamage = GetStat(GameTags.ModOffenseDamage, tags.With(damageType));
            _damage.Add(damageType, baseDamage * effectiveness);
        }
        GD.Print("CalculateDamage: ignoring damage conversion atm");
        return _damage;
    }

    public (float added, float additive, float multiplicative) GetScaling(GameTag stat, TagContainer context)
    {
        if (!modifiers.TryGetValue(stat, out var statModifiers))
            return (0, 0, 1);

        float added = 0, additive = 0, multiplicative = 1;
        foreach (var modifier in statModifiers.Values)
        {
            if (modifier.RequiredTags != null && !modifier.RequiredTags.IsSatisfiedBy(context)) continue;

            switch (modifier.Op)
            {
                case MathOp.Set: return (modifier.Value, 0, 1);
                case MathOp.Added: added += modifier.Value; break;
                case MathOp.Additive: additive += modifier.Value; break;
                case MathOp.Multiplicative: multiplicative *= modifier.Value; break;
            }
        }
        return (added, additive, multiplicative);
    }

    public (float added, float additive, float multiplicative) GetScaling(GameTag stat) => GetScaling(stat, TagContainer.Empty);
}