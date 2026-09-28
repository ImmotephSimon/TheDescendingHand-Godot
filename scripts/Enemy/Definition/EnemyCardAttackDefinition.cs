using Godot;

[GlobalClass]
public partial class EnemyCardAttackDefinition : EnemyAttackDefinition
{
    [Export] private CardDefinition cardDefinition;

    public CardDefinition CardDefinition => cardDefinition;

    public override IEnemyAttack Create(Enemy user)
    {
        var attack = new EnemyCardAttack();
        user.AddChild(attack);
        attack.Initialize(this);
        return attack;
    }
}