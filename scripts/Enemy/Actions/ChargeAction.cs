

[GlobalClass]
public partial class ChargeAttack : EnemyAttack
{
    public override float CooldownDuration => 5f;

    public override void Execute(Node3D target)
    {
        GD.Print($"Charging towards {target.Name}");
    }
}