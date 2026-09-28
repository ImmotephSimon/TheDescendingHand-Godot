[GlobalClass]
public partial class StatModifierData : Resource
{
    [Export] public Godot.Collections.Array<StatModifier> Modifiers { get; set; } = new();

    private readonly Dictionary<IStatContainer, List<ModifierHandle>> _activeBindings = new();

    public void ApplyTo(IStatContainer target)
    {
        if (target == null) return;

        RemoveFrom(target);

        var handles = new List<ModifierHandle>();
        foreach (var mod in Modifiers)
        {
            handles.Add(target.AddModifier(mod));
        }

        _activeBindings[target] = handles;
    }

    public void RemoveFrom(IStatContainer target)
    {
        if (target == null) return;

        if (_activeBindings.TryGetValue(target, out var handles))
        {
            foreach (var handle in handles)
            {
                target.RemoveModifier(handle);
            }
            _activeBindings.Remove(target);
        }
    }

    public void RebuildAllBindings()
    {
        var targets = new List<IStatContainer>(_activeBindings.Keys);

        foreach (var target in targets)
        {
            // Godot lifecycle check replacing UnityEngine.Object null checks
            if (target == null || (target is GodotObject obj && !GodotObject.IsInstanceValid(obj)))
            {
                _activeBindings.Remove(target);
                continue;
            }

            if (_activeBindings.TryGetValue(target, out var handles))
            {
                foreach (var handle in handles)
                {
                    target.RemoveModifier(handle);
                }
                handles.Clear();
            }
            else
            {
                handles = new List<ModifierHandle>();
                _activeBindings[target] = handles;
            }

            foreach (var mod in Modifiers)
            {
                handles.Add(target.AddModifier(mod));
            }
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            _activeBindings.Clear();
        }
    }
}