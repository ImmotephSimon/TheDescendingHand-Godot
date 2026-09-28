[GlobalClass]
public partial class TriggerDetector : Area3D
{
    private readonly Dictionary<IEntity, HashSet<CollisionShape3D>> _inside = new();

    public event Action<IEntity> OnEntered;
    public event Action<IEntity> OnExited;

    public int Count => _inside.Count;

    public override void _Ready()
    {
        AreaShapeEntered += OnAreaShapeEntered;
        AreaShapeExited += OnAreaShapeExited;
        BodyShapeEntered += OnBodyShapeEntered;
        BodyShapeExited += OnBodyShapeExited;
        TreeExiting += OnTreeExiting;
    }

    private void OnAreaShapeEntered(Rid areaRid, Area3D area, long areaShapeIndex, long localShapeIndex)
        => HandleShapeEntered(area, areaShapeIndex);

    private void OnAreaShapeExited(Rid areaRid, Area3D area, long areaShapeIndex, long localShapeIndex)
        => HandleShapeExited(area, areaShapeIndex);

    private void OnBodyShapeEntered(Rid bodyRid, Node3D body, long bodyShapeIndex, long localShapeIndex)
        => HandleShapeEntered(body, bodyShapeIndex);

    private void OnBodyShapeExited(Rid bodyRid, Node3D body, long bodyShapeIndex, long localShapeIndex)
        => HandleShapeExited(body, bodyShapeIndex);

    private void HandleShapeEntered(Node colliderOwner, long shapeIndex)
        => OnTriggerEnter(GetCollisionShapeNode(colliderOwner, shapeIndex));

    private void HandleShapeExited(Node colliderOwner, long shapeIndex)
        => OnTriggerExit(GetCollisionShapeNode(colliderOwner, shapeIndex));

    private void OnTriggerEnter(CollisionShape3D shape)
        => ProcessEnter(shape);

    private void OnTriggerExit(CollisionShape3D shape)
        => ProcessExit(shape);

    private void ProcessEnter(CollisionShape3D shape)
    {
        if (shape == null) return;

        var entity = shape.GetComponentInParent<IEntity>(required: false);
        if (entity == null) return;

        if (!_inside.TryGetValue(entity, out var colliders))
        {
            colliders = new HashSet<CollisionShape3D>();
            _inside.Add(entity, colliders);

            entity.Died += OnEntityDied;
            OnEntered?.Invoke(entity);
        }

        colliders.Add(shape);
    }

    private void ProcessExit(CollisionShape3D shape)
    {
        if (shape == null) return;

        var entity = shape.GetComponentInParent<IEntity>(required: false);
        if (entity == null) return;

        if (!_inside.TryGetValue(entity, out var colliders)) return;

        colliders.Remove(shape);

        if (colliders.Count == 0)
        {
            _inside.Remove(entity);
            entity.Died -= OnEntityDied;
            OnExited?.Invoke(entity);
        }
    }

    private void OnEntityDied(IEntity entity)
    {
        if (!_inside.Remove(entity)) return;

        entity.Died -= OnEntityDied;
        OnExited?.Invoke(entity);
    }

    private void OnTreeExiting()
    {
        AreaShapeEntered -= OnAreaShapeEntered;
        AreaShapeExited -= OnAreaShapeExited;
        BodyShapeEntered -= OnBodyShapeEntered;
        BodyShapeExited -= OnBodyShapeExited;
        TreeExiting -= OnTreeExiting;

        foreach (var entity in _inside.Keys)
            entity.Died -= OnEntityDied;

        _inside.Clear();
    }

    private static CollisionShape3D GetCollisionShapeNode(Node owner, long shapeOwnerIndex)
    {
        if (owner is CollisionObject3D collisionObject)
        {
            var ownerId = collisionObject.ShapeFindOwner((int)shapeOwnerIndex);
            return collisionObject.ShapeOwnerGetOwner(ownerId) as CollisionShape3D;
        }
        return owner as CollisionShape3D;
    }
}