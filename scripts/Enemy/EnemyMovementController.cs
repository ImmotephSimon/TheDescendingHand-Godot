using System;
using Godot;

[GlobalClass]
public partial class EnemyMovementController : Node3D, IEnemyMovement
{
    [Export] private float angularSpeed = 300f;
    [Export] private float stoppingDistance = 0.5f;
    [Export] private float moveSpeed = 3.5f;

    private NavigationAgent3D agent;
    private Vector3 lastDestination;
    private Node3D _lockTarget;

    public override void _Ready()
    {
        agent = this.GetComponentInChildren<NavigationAgent3D>(true) ?? this.GetComponentInOwner<NavigationAgent3D>(true);
        if (agent == null) GD.PrintErr($"NavigationAgent3D missing on {Name}!");

        agent.TargetDesiredDistance = stoppingDistance;
        SetProcess(false);
    }

    public void MoveTo(Vector3 position)
    {
        if (lastDestination.DistanceSquaredTo(position) < 0.25f)
            return;

        lastDestination = position;
        agent.TargetPosition = position;
        SetProcess(true);
    }

    public void StopMovement()
    {
        agent.TargetPosition = GlobalPosition;
        SetProcess(false);
    }

    public override void _Process(double delta)
    {
        if (agent.IsNavigationFinished())
        {
            SetProcess(false);
            return;
        }

        Vector3 nextPos = agent.GetNextPathPosition();
        Vector3 direction = nextPos - GlobalPosition;
        direction.Y = 0f;

        if (direction.LengthSquared() > 0.001f)
        {
            GlobalPosition += direction.Normalized() * moveSpeed * (float)delta;
        }
    }

    public bool HasReachedDestination()
    {
        return agent.IsNavigationFinished();
    }

    public bool IsWithinStoppingDistance(Vector3 position)
    {
        return GlobalPosition.DistanceTo(position) <= agent.TargetDesiredDistance;
    }

    public void RotateTowardsTarget(Node3D target)
    {
        Node3D focus = _lockTarget ?? target;
        if (focus == null) return;

        Vector3 direction = focus.GlobalPosition - GlobalPosition;
        direction.Y = 0f;

        if (direction.LengthSquared() < 0.001f)
            return;

        Basis targetBasis = Basis.LookingAt(-direction, Vector3.Up);
        Transform = new Transform3D(
            Transform.Basis.Slerp(targetBasis, angularSpeed * (float)GetProcessDeltaTime() * Mathf.DegToRad(1f)),
            GlobalPosition
        );
    }

    public float GetFacingAngle(Node3D target)
    {
        Vector3 direction = target.GlobalPosition - GlobalPosition;
        direction.Y = 0f;

        if (direction.LengthSquared() < 0.001f)
            return 0f;

        return Mathf.RadToDeg(Transform.Basis.Z.AngleTo(-direction.Normalized()));
    }

    public void LockRotation(Node3D target) => _lockTarget = target;
    public void UnlockRotation() => _lockTarget = null;
}