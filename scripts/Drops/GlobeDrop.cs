[GlobalClass]
public partial class GlobeDrop : WorldDrop
{
    private float _healPercentage;

    public void Initialize(float healPercentage) => _healPercentage = healPercentage;

    public override void _Ready() => BodyEntered += OnBodyEntered;

    private void OnBodyEntered(Node3D body)
    {
        if (!Multiplayer.IsServer() || !body.TryGetComponent<IHealth>(out var health)) return;
        health.AdjustHealth(health.MaxHealth * _healPercentage, this);
        QueueFree();
    }
}

