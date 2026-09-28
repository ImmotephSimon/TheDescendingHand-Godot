using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Pushable : Area3D
{
    private RigidBody3D rb;
    [Export] private float pushForce = 700f;
    private readonly List<Node3D> interactingEntities = new();

    public override void _Ready()
    {
        rb = GetParent<RigidBody3D>() ?? GetNodeOrNull<RigidBody3D>(".");
        if (rb == null) GD.PrintErr($"No RigidBody3D found for {Name}.");

        BodyEntered += body => { if (body is IEntity && !interactingEntities.Contains(body)) interactingEntities.Add(body); };
        BodyExited += body => { if (body is IEntity) interactingEntities.Remove(body); };
    }

    public override void _PhysicsProcess(double delta)
    {
        if (rb == null || interactingEntities.Count == 0) return;

        foreach (var entity in interactingEntities)
        {
            if (entity == null) continue;
            Vector3 pushDir = GlobalPosition - entity.GlobalPosition;
            pushDir.Y = 0;
            rb.ApplyCentralForce(pushDir.Normalized() * pushForce);
        }
    }
}