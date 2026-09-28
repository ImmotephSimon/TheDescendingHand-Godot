[GlobalClass]
public partial class AreaOverlapComponent : CardComponent
{
    private Area3D _collider;

    public event Action<IEntity> OnEntityEntered;
    public event Action<IEntity> OnEntityExited;

    public void Configure(Area3D collider)
    {
        _collider = collider;
        _collider.BodyEntered += body => { if (body is IEntity e) OnEntityEntered?.Invoke(e); };
        _collider.BodyExited += body => { if (body is IEntity e) OnEntityExited?.Invoke(e); };
        OnEntityEntered += entity => Card.OnHit?.Invoke(new HitInfo(entity, CardOwner, Card.GlobalPosition));
    }

    protected override void OnActivate()
    {
        base.OnActivate();
        if (_collider != null) _collider.Monitoring = true;
    }

    public void Attach(Node3D targetTransform)
    {
        Card.Reparent(targetTransform, keepGlobalTransform: false);
        Card.Position = Vector3.Zero;
        if (_collider != null) _collider.Monitoring = true;
    }

    protected override void OnCancel()
    {
        if (_collider != null) _collider.Monitoring = false;
    }
}