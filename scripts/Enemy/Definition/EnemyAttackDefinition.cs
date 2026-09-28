[GlobalClass]
public abstract partial class EnemyAttackDefinition : Resource
{
    [Export] public float Range { get; set; } = 1.5f;
    [Export] public float Cooldown { get; set; } = 1f;
    [Export] public AttackAnimation Animation { get; set; }
    [Export] public float Priority { get; set; } = 100f;

    public abstract IEnemyAttack Create(Enemy user);
}