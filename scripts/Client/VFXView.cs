[GlobalClass]
public partial class VFXView : Node
{
    private readonly Dictionary<Node3D, List<Node3D>> _activeVisuals = new();

    public void AttachAbilityVisual(PackedScene prefab, Node3D target)
    {
        if (prefab == null)
        {
            GD.PushWarning("No VFX prefab provided.");
            return;
        }

        if (target == null) return;

        var instance = prefab.Instantiate<Node3D>();
        target.AddChild(instance);
        instance.Position = Vector3.Zero;
        instance.Basis = Basis.Identity;

        if (instance is IVfx vfx)
        {
            var spawnParams = new VfxSpawnParams(target.GlobalPosition)
            {
                Rotation = target.GlobalTransform.Basis.GetRotationQuaternion()
            };

            vfx.Initialize(spawnParams, target);
        }

        if (!_activeVisuals.TryGetValue(target, out var list))
        {
            list = new List<Node3D>();
            _activeVisuals[target] = list;
        }
        list.Add(instance);
    }

    public void DetachAbilityVisual(PackedScene prefab, Node3D target)
    {
        if (target == null || !_activeVisuals.TryGetValue(target, out var list)) return;

        for (int i = list.Count - 1; i >= 0; i--)
        {
            var instance = list[i];
            if (GodotObject.IsInstanceValid(instance))
            {
                if (instance is IVfx vfx)
                {
                    GD.Print($"Client {prefab?.ResourcePath ?? "VFX"} stop following: {target.Name}");
                    vfx.Stop();
                }
                else
                {
                    instance.QueueFree();
                }
            }
        }

        _activeVisuals.Remove(target);
    }
}