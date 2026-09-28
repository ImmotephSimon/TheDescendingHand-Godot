[GlobalClass]
public partial class AttackHitbox : Node3D
{
    [Export] private Area3D _hitArea;
    public event Action<IEntity> OnHit;

    public override void _Ready()
    {
        _hitArea.Monitoring = false;
        _hitArea.BodyEntered += OnBodyEntered;
    }

    public void Enable()
    {
        _hitArea.Monitoring = true;
    }

    public void Disable()
    {
        _hitArea.Monitoring = false;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.TryGetComponentInParent<IEntity>(out var target))
        {
            OnHit?.Invoke(target);
        }
    }
}