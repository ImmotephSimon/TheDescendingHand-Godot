[GlobalClass]
public partial class EnemyAttack : Node, IEnemyAttack
{
    protected AttackAnimation attackAnimation;
    protected float range;
    protected float cooldownDuration;
    protected float priority;

    public event Action<IEntity> OnHit;
    public virtual float Range => range;
    public virtual float CooldownDuration => cooldownDuration;
    public virtual AttackAnimation AttackAnimation => attackAnimation;
    public virtual float Effectiveness => 1f;

    protected AttackHitbox AttackHitbox => attackHitbox;
    private AttackHitbox attackHitbox;

    public virtual void Initialize(EnemyAttackDefinition def)
    {
        range = def.Range;
        cooldownDuration = def.Cooldown;
        attackAnimation = def.Animation;
        priority = def.Priority;

        attackHitbox = this.GetComponentInChildren<AttackHitbox>();
        if (attackHitbox != null)
        {
            attackHitbox.OnHit += HandleHit;
        }
    }

    public float GetPriority() => priority;

    private void HandleHit(IEntity entity) => OnHit?.Invoke(entity);

    public virtual bool CanHit(Node3D target)
    {
        var enemyPos = GetParent<Node3D>().GlobalPosition;
        Vector3 direction = target.GlobalPosition - enemyPos;
        direction.Y = 0f;
        return direction.Length() <= Range;
    }

    public virtual void Execute(Node3D target)
    {
    }

    public virtual void Stop()
    {
    }

    public virtual void OnAnimationFinish()
    {
    }
}