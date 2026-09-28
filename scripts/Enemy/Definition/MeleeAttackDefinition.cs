[GlobalClass]
public partial class MeleeAttackDefinition : EnemyAttackDefinition
{
    public override IEnemyAttack Create(Enemy owner)
    {
        var attack = new MeleeAttack();
        owner.AddChild(attack);
        attack.Initialize(this);
        return attack;
    }
}