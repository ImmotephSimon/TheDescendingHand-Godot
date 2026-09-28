[GlobalClass]
public partial class TargetDummy : Entity
{
    private EnemyHealthBar _healthBar;

    protected override Node ComponentRoot => this;

    public override void _Ready()
    {
        base._Ready();

        _stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth,
                MathOp.Added,
                20f));

        Callable.From(Start).CallDeferred();
    }

    private void Start()
    {
        if (_healthBar == null)
        {
            _healthBar = this.GetComponentInChildren<EnemyHealthBar>();
            _healthBar?.Bind(this.GetComponentInOwner<IHealth>());
        }
    }

    public override void TakeDamage(DamageInfo info)
    {
        base.TakeDamage(info);
        HandleTargetDummyHit();
    }

    private void HandleTargetDummyHit()
    {

    }

    public override void ApplyDegen(DegenInfo degenInfo)
    {
        base.ApplyDegen(degenInfo);
        HandleTargetDummyHit();
    }

    protected override void OnDeath(IEntity killer)
    {
        GameWorld.Instance.NotifyRevive(this);
    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
    }

    protected override void OnEntityRevived(IEntity entity)
    {
        IsDead = false;
        this.GetComponentInOwner<IHealth>()?.AdjustHealth(_stats.GetStat(GameTags.ModStatHealth), this);
    }
}