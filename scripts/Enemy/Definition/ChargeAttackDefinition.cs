[GlobalClass]
public partial class ChargeAttackDefinition : EnemyAttackDefinition
{
    public override IEnemyAttack Create(Enemy user)
    {
        var attack = new ChargeAttack();
        user.AddChild(attack);
        attack.Initialize(this);
        return attack;
    }

}