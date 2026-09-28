using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class Perception : Node3D, IPerception
{
    [Export] private float _sightRange = 15f;
    [Export(PropertyHint.Layers3DPhysics)] private uint _targetMask = 1;
    [Export(PropertyHint.Layers3DPhysics)] private uint _obstacleMask = 1;
    [Export] private float _scanInterval = 0.1f;
    [Export] private Node3D _eyePoint;
    [Export] private int _maxDetectedColliders = 32;
    [Export] private float _forgetDuration = 3f;

    private Node3D _currentTarget;
    private float _nextScanTime;
    private float _lastTimeTargetSeen;
    private readonly HashSet<Node> _invalidTargets = new();

    public bool HasTarget => _currentTarget != null;
    public Node3D Target => _currentTarget;

    public override void _Ready()
    {
        if (_eyePoint == null) _eyePoint = this;
    }

    public override void _Process(double delta)
    {
        float currentTime = Time.GetTicksMsec() / 1000f;
        if (currentTime >= _nextScanTime)
        {
            _nextScanTime = currentTime + _scanInterval;
            FindTarget(currentTime);
        }
    }

    private void FindTarget(float currentTime)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var shape = new SphereShape3D { Radius = _sightRange };
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            Transform = _eyePoint.GlobalTransform,
            CollisionMask = _targetMask,
            CollideWithBodies = true,
            CollideWithAreas = true
        };

        var results = spaceState.IntersectShape(query, _maxDetectedColliders);
        float closestDistanceSqr = float.MaxValue;
        Node3D bestTarget = null;

        foreach (var result in results)
        {
            if (result["collider"].As<Node3D>() is not Node3D hitNode || _invalidTargets.Contains(hitNode))
                continue;

            float distanceSqr = _eyePoint.GlobalPosition.DistanceSquaredTo(hitNode.GlobalPosition);
            if (distanceSqr < closestDistanceSqr)
            {
                var rayQuery = PhysicsRayQueryParameters3D.Create(_eyePoint.GlobalPosition, hitNode.GlobalPosition, _obstacleMask);
                var rayResult = spaceState.IntersectRay(rayQuery);
                if (rayResult.Count > 0) continue;

                closestDistanceSqr = distanceSqr;
                bestTarget = hitNode;
            }
        }

        if (bestTarget != null)
        {
            _currentTarget = bestTarget;
            _lastTimeTargetSeen = currentTime;
        }
        else if (_currentTarget != null && (currentTime - _lastTimeTargetSeen >= _forgetDuration))
        {
            _currentTarget = null;
        }
    }

    public void SetTargetValidity(Node target, bool isValid)
    {
        if (isValid) _invalidTargets.Remove(target);
        else
        {
            _invalidTargets.Add(target);
            if (_currentTarget == target) _currentTarget = null;
        }
    }
}