[GlobalClass]
public partial class DebrisCollisionNotifier : RigidBody3D
{
    private Action<Node3D, Vector3> _onHit;
    private float _minVelocity = 2f;

    public async void Initialize(Action<Node3D, Vector3> onHit, float lifetime = 2.5f)
    {
        _onHit = onHit;
        ContactMonitor = true;
        MaxContactsReported = 4;
        BodyEntered += OnBodyEntered;
        await ToSignal(GetTree().CreateTimer(lifetime), SceneTreeTimer.SignalName.Timeout);
        QueueFree();
    }

    private void OnBodyEntered(Node body)
    {
        if (LinearVelocity.Length() < _minVelocity) { QueueFree(); return; }
        if (body is Node3D node3D && !body.IsInGroup("Debris"))
        {
            _onHit?.Invoke(node3D, GlobalPosition);
            QueueFree();
        }
    }
}