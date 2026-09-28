using Godot;
using System.Collections.Generic;
using System.Linq;

[GlobalClass]
public partial class FractureComponent : Node3D
{
    [Export(PropertyHint.Layers3DPhysics)] public uint DebrisLayer { get; set; }
    [Export(PropertyHint.Layers3DPhysics)] public uint DebrisMask { get; set; }

    private CollisionShape3D _rootCollider;
    private RigidBody3D _rootRb;
    private List<RigidBody3D> _debrisRbs = new();
    private List<CollisionShape3D> _debrisColliders = new();

    public override void _Ready() => Initialize();

    public void Initialize()
    {
        _rootCollider = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        _rootRb = GetParentOrNull<RigidBody3D>() ?? GetNodeOrNull<RigidBody3D>(".");

        Node debrisTarget = GetNodeOrNull("Debris") ?? this;

        _debrisRbs = debrisTarget.FindChildren("*", "RigidBody3D", recursive: true)
            .OfType<RigidBody3D>()
            .Where(rb => rb != _rootRb)
            .ToList();

        _debrisColliders = _debrisRbs
            .SelectMany(rb => rb.GetChildren().OfType<CollisionShape3D>())
            .ToList();

        foreach (var rb in _debrisRbs)
        {
            if (DebrisLayer != 0) rb.CollisionLayer = DebrisLayer;
            if (DebrisMask != 0) rb.CollisionMask |= DebrisMask;
        }

        SetState(isShattered: false);
    }

    public void Break(Vector3 hitPoint, float force = 2f, float radius = 0.5f, float upwardModifier = 0f)
    {
        SetState(isShattered: true);

        foreach (var rb in _debrisRbs)
        {
            rb.LinearVelocity = Vector3.Zero;
            rb.AngularVelocity = Vector3.Zero;

            Vector3 dir = rb.GlobalPosition - hitPoint;
            float dist = dir.Length();
            if (radius > 0f && dist > radius) continue;

            dir = dir.LengthSquared() > 0 ? dir.Normalized() : Vector3.Up;
            dir.Y += upwardModifier;

            float falloff = radius > 0f ? (1f - Mathf.Clamp(dist / radius, 0f, 1f)) : 1f;
            rb.ApplyImpulse(dir.Normalized() * force * falloff);
        }
    }

    public void ResetState() => SetState(isShattered: false);

    private void SetState(bool isShattered)
    {
        if (_rootCollider != null) _rootCollider.Disabled = isShattered;
        if (_rootRb != null) _rootRb.Freeze = isShattered;

        foreach (var col in _debrisColliders)
            col.Disabled = !isShattered;

        foreach (var rb in _debrisRbs)
            rb.Freeze = !isShattered;
    }
}