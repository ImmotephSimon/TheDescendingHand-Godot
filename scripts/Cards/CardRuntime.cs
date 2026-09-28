[GlobalClass]
public partial class CardRuntime : Node3D
{
    public IEntity CardOwner => _cardOwner;
    public Guid Id { get; private set; }
    public CardDefinition Definition { get; private set; }
    public float CastTime => Definition.CastTime;
    public bool SpawnAtCursor => Definition.SpawnAtCursor;
    public Vector3 TargetLocation { get; private set; }
    public TagContainer Tags { get; } = new();

    public bool IsTicking => _components.Exists(c => c.IsTicking);

    private IEntity _cardOwner;
    private readonly List<CardComponent> _components = new();

    public event Action OnActivated;
    public Action<HitInfo> OnHit;

    public void Initialize(Guid id, CardDefinition definition, IEntity owner)
    {
        Id = id;
        Definition = definition;
        _cardOwner = owner;
    }

    public T AddCardComponent<T>() where T : CardComponent, new()
    {
        var component = new T();
        component.Initialize(this, _cardOwner);

        AddChild(component);

        Tags.AddRange(component.GetTags());
        _components.Add(component);

        return component;
    }

    public void SetTargetLocation(Vector3 location) => TargetLocation = location;

    public T GetCardComponent<T>() where T : CardComponent
    {
        for (int i = 0; i < _components.Count; i++)
        {
            if (_components[i] is T typed) return typed;
        }
        return null;
    }

    public void ExecuteCastTimeDone()
    {
        GlobalPosition = TargetLocation;
        Basis = GetLookRotation(TargetLocation);

        OnActivated?.Invoke();
        foreach (var comp in _components)
            comp.Activate();
    }

    public void ExecuteCancelled()
    {
        foreach (var comp in _components)
            comp.Cancel();
    }

    private Basis GetLookRotation(Vector3 target)
    {
        Vector3 direction = target - _cardOwner.Node.Position;
        direction.Y = 0f;

        if (direction.LengthSquared() < 0.0001f)
            return Basis.Identity;

        // Godot forward vector points toward -Z
        return Transform3D.Identity.LookingAt(-direction, Vector3.Up).Basis;
    }

    public AreaOverlapComponent AddSphereOverlap(float radius)
    {
        var area = new Area3D { CollisionLayer = _cardOwner.AttackLayer };
        area.AddChild(new CollisionShape3D { Shape = new SphereShape3D { Radius = radius } });
        AddChild(area);

        var overlap = AddCardComponent<AreaOverlapComponent>();
        overlap.Configure(area);
        return overlap;
    }

    public AreaOverlapComponent AddBoxOverlap(Vector3 size)
    {
        var area = new Area3D { CollisionLayer = _cardOwner.AttackLayer };
        area.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        AddChild(area);

        var overlap = AddCardComponent<AreaOverlapComponent>();
        overlap.Configure(area);
        return overlap;
    }
}