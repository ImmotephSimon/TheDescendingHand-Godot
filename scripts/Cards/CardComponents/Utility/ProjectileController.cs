[GlobalClass]
public partial class ProjectileController : Node3D
{
    private static uint _environmentMask;
    private uint DynamicHitMask => _environmentMask | (1u << (int)(_owner != null ? _owner.HostileLayer : 0));

    private string _vfxId = string.Empty;

    private Vector3 _velocity;
    private bool _initialized;
    private float _colliderRadius = 0.5f;
    private ProjectileInfo _info;
    private IEntity _owner;
    private float _chainRadius = 4f;
    private IEntity _lastHitEntity;
    private PackedScene _visual;
    private readonly float _maxProjectileDistanceSqr = 1800f;

    public event Action<HitInfo> OnHit;
    public event Action OnDespawn;

    public override void _Ready()
    {
        var shapeNode = this.GetComponentInChildren<CollisionShape3D>(required: false);
        if (shapeNode?.Shape is SphereShape3D sphereShape)
        {
            _colliderRadius = sphereShape.Radius;
        }

        if (_environmentMask == 0)
        {
            // Default environment collision mask layers (adjust as needed for Wall, Default, Floor)
            _environmentMask = (1u << 0) | (1u << 1) | (1u << 2);
        }
    }

    public void Initialize(ProjectileInfo info, IEntity owner)
    {
        _info = info;
        _owner = owner;
        _velocity = _info.GetLaunchVelocity(_owner);
        _initialized = true;

        if (Multiplayer.IsServer())
        {
            StartServerTick();
        }
    }

    private async void StartServerTick()
    {
        while (IsInsideTree() && _initialized)
        {
            if (!Multiplayer.IsServer()) break;

            if (_owner != null && GlobalPosition.DistanceSquaredTo(_owner.Node.GlobalPosition) >= _maxProjectileDistanceSqr)
            {
                End();
                return;
            }

            float gravityValue = ProjectSettings.GetSetting("physics/3d/default_gravity").As<float>();
            _velocity += Vector3.Down * gravityValue * _info.Direction.Gravity * (float)GetProcessDeltaTime();
            Vector3 movement = _velocity * (float)GetProcessDeltaTime();

            CheckCollision(GlobalPosition, movement);
            GlobalPosition += movement;

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }

    private void End()
    {
        OnDespawn?.Invoke();
        QueueFree();
    }

    private void CheckCollision(Vector3 start, Vector3 movement)
    {
        float distance = movement.Length();
        if (distance <= 0f) return;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters3D();
        var sphere = new SphereShape3D { Radius = _colliderRadius };
        query.Shape = sphere;
        query.Transform = new Transform3D(Basis.Identity, start);
        query.Motion = movement;
        query.CollisionMask = DynamicHitMask;

        var castResult = spaceState.CastMotion(query);
        if (castResult.Length > 0 && castResult[0] < 1f)
        {
            float safeFraction = castResult[0];
            Vector3 hitPoint = start + movement * safeFraction;

            query.Transform = new Transform3D(Basis.Identity, hitPoint);
            query.Motion = Vector3.Zero;
            var hitResults = spaceState.IntersectShape(query, 1);

            Node hitNode = hitResults.Count > 0 ? hitResults[0]["collider"].As<Node>() : null;

            if (hitNode != null && hitNode.TryGetComponent<IEntity>(out var target))
            {
                if (TryHit(target))
                {
                    if (_info.Pierce > 0)
                    {
                        _info.Pierce--;
                        GD.Print($"Piercing {target.Node.Name}");
                        return;
                    }

                    if (TryChain(hitPoint)) return;
                    RpcOnTerminalHit();
                }
            }
            else
            {
                Vector3 normal = Vector3.Up;
                if (TryBounce(hitPoint, normal))
                {
                    return;
                }
                else
                {
                    OnHit?.Invoke(new HitInfo(null, _owner, hitPoint));
                }
            }
            End();
        }
    }

    private bool TryHit(IEntity target)
    {
        if (target == _owner || target == _lastHitEntity || target.IsDead)
            return false;

        _lastHitEntity = target;
        GD.Print($"PROJECTILE HIT {target.Node.Name} id={target.Node.GetInstanceId()} frame={Engine.GetFramesDrawn()}");
        OnHit?.Invoke(new HitInfo(target, _owner, GlobalPosition));

        return true;
    }

    private bool TryChain(Vector3 hitPoint)
    {
        if (_info.Chains <= 0) return false;

        if (!TryFindNextTarget(hitPoint, out Vector3 nextDir))
            return false;

        _info.Chains--;
        GlobalPosition = hitPoint;
        Redirect(nextDir);
        return true;
    }

    private bool TryBounce(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (_info.Bounces <= 0) return false;

        _info.Bounces--;
        GlobalPosition = hitPoint + hitNormal * 0.01f;

        Vector3 reflectedDir = _velocity.Bounce(hitNormal).Normalized();
        Redirect(reflectedDir);
        return true;
    }

    private void Redirect(Vector3 newDirection)
    {
        _velocity = newDirection * _velocity.Length();
        GlobalTransform = new Transform3D(Basis.LookingAt(newDirection, Vector3.Up), GlobalPosition);

        RpcOnRedirect(GlobalPosition, GlobalTransform.Basis.GetRotationQuaternion());
    }

    private bool TryFindNextTarget(Vector3 origin, out Vector3 direction)
    {
        direction = Vector3.Zero;

        uint targetMask = 1u << (int)_owner.HostileLayer;
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters3D();
        var sphere = new SphereShape3D { Radius = _chainRadius };
        query.Shape = sphere;
        query.Transform = new Transform3D(Basis.Identity, origin);
        query.CollisionMask = targetMask;

        var results = spaceState.IntersectShape(query);

        Node nearest = null;
        float minDist = float.MaxValue;

        foreach (var result in results)
        {
            var col = result["collider"].As<Node>();
            if (col == null || !col.TryGetComponent<IEntity>(out var entity)) continue;
            if (entity == _owner || entity == _lastHitEntity || entity.IsDead) continue;

            float dist = GlobalPosition.DistanceSquaredTo(col is Node3D n3d ? n3d.GlobalPosition : origin);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col;
            }
        }

        if (nearest == null) return false;

        Vector3 targetPos = nearest is Node3D targetNode3d ? targetNode3d.GlobalPosition : origin;
        direction = (targetPos - origin).Normalized();
        return true;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RpcOnRedirect(Vector3 point, Godot.Quaternion rotation)
    {
        ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual, this);
        GlobalPosition = point;
        GlobalTransform = new Transform3D(new Basis(rotation), point);
        ClientBridge.Instance.VFXView.AttachAbilityVisual(_visual, this);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RpcOnTerminalHit()
    {
        ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual, this);
    }

    public override void _ExitTree()
    {
        if (_visual != null)
        {
            ClientBridge.Instance.VFXView.DetachAbilityVisual(_visual, this);
        }
    }

    public void SetVfxId(string vfxId)
    {
        _vfxId = vfxId;
        if (!Multiplayer.IsServer())
        {
            if (GameData.Instance.Vfx.TryGetPrefab(_vfxId, out _visual))
            {
                ClientBridge.Instance.VFXView.AttachAbilityVisual(_visual, this);
            }
        }
    }
}