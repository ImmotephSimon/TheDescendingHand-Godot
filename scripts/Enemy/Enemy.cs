[GlobalClass]
public partial class Enemy : Entity, IExperienceSource
{
    [Export] private bool debug = false;
    [Export] private ExperienceTable experienceTable;
    [Export] private BalanceCurves balanceCurves;

    [Export] private EnemyDefinition definition;
    [Export] private Node3D visualRoot;
    [Export] private PackedScene healthBarPrefab;

    private float patrolRadius = 10f;

    private EnemyBrain brain;
    private Perception perception;
    private Rarity rarity;
    private int baseExperience = 1;
    private int _level = 1;
    private IHealthBar _healthBar;
    public int ExperienceReward => experienceTable.ScaleByRarity(baseExperience, rarity);

    protected override Node ComponentRoot => this;

    public override Vector3 GetCursorPosition() => perception.Target.GlobalPosition;

    public override void _Ready()
    {
        base._Ready();

        brain = this.GetComponentInOwner<EnemyBrain>();
        perception = this.GetComponentInOwner<Perception>();

        if (debug) Initialize(definition, 1);

        // Deferred call to run next frame (Unity's Start timing)
        Callable.From(Start).CallDeferred();
    }

    public void Initialize(EnemyDefinition definition, int enemyLevel)
    {
        this.definition = definition;
        _level = enemyLevel;

        if (definition.ModelPrefab != null && visualRoot.GetChildCount() == 0)
        {
            var model = definition.ModelPrefab.Instantiate();
            visualRoot.AddChild(model);
        }

        AttachAttacks(definition.Attacks);

        brain = this.GetComponentInOwner<EnemyBrain>();
        perception = this.GetComponentInOwner<Perception>();
        _animationHandler = this.GetComponentInChildren<IAnimationHandler>();
        Debug.Assert(_animationHandler != null, "Failed to find animation handler.");

        RegisterActions();
        ApplyBaseStats();
    }

    private void Start()
    {
        if (definition == null) return;

        if (definition.EnemyType == EnemyType.Unique)
        {
            _healthBar = ClientBridge.Instance.PlayerHUD.BindBossHealthBar(this);
        }
        else if (healthBarPrefab != null)
        {
            var barNode = healthBarPrefab.Instantiate();
            AddChild(barNode);
            _healthBar = barNode.GetComponentInOwner<EnemyHealthBar>();
            _healthBar.Bind(this.GetComponentInOwner<IHealth>());
        }
    }

    private void AttachAttacks(Godot.Collections.Array<EnemyAttackDefinition> attackDefinitions)
    {
        if (attackDefinitions == null) return;

        foreach (var definition in attackDefinitions)
        {
            if (definition != null)
            {
                definition.Create(this);
            }
        }
    }

    private void ApplyBaseStats()
    {
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth,
                MathOp.Added,
                20));
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModOffenseDamage,
                MathOp.Added,
                balanceCurves.ExpectedPlayerLife.Sample(_level) * 0.1f));
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatStunThreshold,
                MathOp.Added,
                0.2f));
        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatStunDuration,
                MathOp.Added,
                1f));
    }

    private void RegisterActions()
    {
        brain.AddAction(new ChaseAction(this));
        brain.AddAction(new PatrolAction(this, patrolRadius));
        foreach (var ability in this.GetComponentsInChildren<IEnemyAttack>())
        {
            brain.AddAction(new AttackAction(this, ability));
        }
    }

    protected override void OnDeath(IEntity killer)
    {
        brain.SetState(BrainState.Dead);

        foreach (var col in this.GetComponentsInChildren<CollisionShape3D>())
            col.SetDeferred(CollisionShape3D.PropertyName.Disabled, true);

        if (definition.EnemyType == EnemyType.Unique)
            ClientBridge.Instance.PlayerHUD.UnbindBossHealthBar(this);
        else if (_healthBar is Node healthBarNode)
            healthBarNode.QueueFree();

        this.GetComponentInOwner<DropsComponent>().DropFromEnemy(GlobalPosition);
    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (victim.HostileLayer == TeamLayer)
        {
            GD.Print($"Disable AI on {killer}");
            perception.SetTargetValidity(victim.Node, false);
        }
    }

    protected override void OnEntityRevived(IEntity entity)
    {
        base.OnEntityRevived(entity);

        if (entity.HostileLayer == TeamLayer)
        {
            GD.Print($"Enable AI on {entity} revival");
            perception.SetTargetValidity(entity.Node, true);
        }
    }

    protected override void OnStunBegin()
    {
        brain.SetState(BrainState.Suspended);
    }

    protected override void OnStunEnd()
    {
        brain.SetState(BrainState.Active);
    }
}