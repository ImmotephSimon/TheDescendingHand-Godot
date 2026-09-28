using System.Threading.Tasks;

public abstract partial class Entity : Node3D, IEntity, IDamageable, IStunnable
{
    [Export] private TagReactions tagReactions;

    protected IAnimationHandler _animationHandler;
    protected IStatContainer _stats;
    protected DegenComponent _degen;
    private Task _stunTask;
    private IHealth _healthHandler;
    private IAilmentHandler _ailmentHandler;
    private ModifierHandle _stunStatHandle;
    private DamagePipelineComponent _damagePipeline;

    [Export(PropertyHint.Layers3DPhysics)] private uint _teamLayer = 1;
    public uint TeamLayer => _teamLayer;

    public uint HostileLayer => TeamLayer == 1u ? 2u : 1u;
    public uint AttackLayer => TeamLayer == 1u ? 4u : 8u;

    public bool IsDead { get; protected set; }
    public Node3D Node => this;
    public IStatContainer Stats => _stats;
    public List<(StatModifier, float)> OnHitStats { get; } = new();
    protected abstract Node ComponentRoot { get; }

    public event Action<IEntity> Died;

    public override void _EnterTree()
    {
        base._EnterTree();

        _degen = AddComponent<DegenComponent>();
        _damagePipeline = AddComponent<DamagePipelineComponent>();
        _stats = ComponentRoot.GetComponentInChildren<IStatContainer>();
        _ailmentHandler = ComponentRoot.GetComponentInChildren<IAilmentHandler>();
        _healthHandler = ComponentRoot.GetComponentInChildren<IHealth>();

        foreach (var reaction in tagReactions.Reactions)
            reaction.StartListening(this);

        GameWorld.Instance.RegisterEntity(this);
        GameWorld.Instance.EntityDied += OnEntityDied;
        GameWorld.Instance.EntityRevived += OnEntityRevived;
    }


    private T AddComponent<T>() where T : Node, new()
    {
        var nodePath = new NodePath(typeof(T).Name);

        if (HasNode(nodePath))
            return GetParent().GetNode<T>(nodePath);

        var component = new T
        {
            Name = typeof(T).Name
        };
        AddChild(component);
        component.Owner = this;

        return component;
    }

    private T GetNodeRecursive<T>(Node node) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T match) return match;
            var deepMatch = GetNodeRecursive<T>(child);
            if (deepMatch != null) return deepMatch;
        }
        return null;
    }

    public override void _ExitTree()
    {
        if (GameWorld.Instance == null) return;
        GameWorld.Instance.UnregisterEntity(this);
        GameWorld.Instance.EntityDied -= OnEntityDied;
        GameWorld.Instance.EntityRevived -= OnEntityRevived;
    }

    protected virtual void OnEntityRevived(IEntity entity) { }
    protected virtual void OnEntityDied(IEntity victim, IEntity killer) { }

    public void Die(IEntity killer)
    {
        if (IsDead) return;
        if (killer == null) GD.PrintS($"Die {Name} with NULL killer");

        IsDead = true;
        if (_stunTask != null)
        {
            GD.PrintS($"CANCEL STUN {Name}");
            _stats.RemoveModifier(_stunStatHandle);
            _stunTask = null;
        }
        OnDeath(killer);
        Died?.Invoke(this);
        _animationHandler?.SetAnimationState(CharacterAnimationState.Dead);
        GameWorld.Instance.NotifyDeath(this, killer);
    }

    public virtual void TakeDamage(DamageInfo info)
    {
        foreach ((StatModifier stat, float duration) in info.Source.OnHitStats)
        {
            _stats.AddModifier(stat, duration);
        }

        if (!_damagePipeline.RunPreProcess(this, info, out float finalDamage))
        {
            GD.Print("[Entity:TakeDamage] Something logical prevented damage application.");
            return;
        }

        _healthHandler.AdjustHealth(-finalDamage, info.Source);
        _ailmentHandler.ApplyAilments(info);
        _damagePipeline.RunPostProcess(this, info, finalDamage);
    }

    public virtual void ApplyStun(float duration)
    {
        if (IsDead) return;
        if (_stunTask != null)
        {
            _stats.RemoveModifier(_stunStatHandle);
        }
        OnStunBegin();
        _stunStatHandle = _stats.AddModifier(new StatModifier(GameTags.StatusStun, MathOp.Added, 1));
        _stunTask = StunRoutine(duration);
    }

    private async Task StunRoutine(float duration)
    {
        await Task.Delay(TimeSpan.FromSeconds(duration));
        _stunTask = null;
        if (IsDead) return;

        _stats.RemoveModifier(_stunStatHandle);
        OnStunEnd();
    }

    protected virtual void OnDeath(IEntity killer) { }
    protected virtual void OnStunBegin() { }
    protected virtual void OnStunEnd() { }

    public virtual void ApplyDegen(DegenInfo degenInfo)
    {
        var damageInfo = new DamageInfo(degenInfo.Damage, degenInfo.Source, degenInfo.Position);

        if (!_damagePipeline.RunPreProcess(this, damageInfo, out float finalDamage))
        {
            GD.Print("[Entity:TakeDamage] Something logical prevented damage application.");
            return;
        }

        degenInfo.Damage = damageInfo.DamageMap;
        _degen.Apply(degenInfo);
    }

    public void RemoveDegen(Guid id)
    {
        _degen.RemoveDegen(id);
    }

    public virtual Vector3 GetCursorPosition()
    {
        throw new NotImplementedException();
    }
}