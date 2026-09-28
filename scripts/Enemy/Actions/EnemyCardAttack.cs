using Godot;
using System;

[GlobalClass]
public partial class EnemyCardAttack : EnemyAttack
{
    private CardInstance _card;
    private CardRuntime _runtime;

    public override float CooldownDuration => 7f;
    public override AttackAnimation AttackAnimation => AttackAnimation.Special;

    public override void Initialize(EnemyAttackDefinition def)
    {
        base.Initialize(def);

        if (def is not EnemyCardAttackDefinition cardDef)
        {
            GD.PrintErr($"[{Name}] EnemyCardAttack initialized with invalid type: {def?.GetType().Name ?? "null"}");
            return;
        }

        if (cardDef.CardDefinition == null)
        {
            GD.PrintErr($"[{Name}] EnemyCardAttackDefinition '{cardDef.ResourceName}' is missing its CardDefinition!");
            return;
        }

        var owner = this.GetComponentInOwner<IEntity>() ?? this.GetComponentInParent<IEntity>();
        _card = CardFactory.CreateCardInstance(cardDef.CardDefinition, owner);
    }

    public override void Execute(Node3D target)
    {
        _runtime = CardFactory.CreateRuntime(_card, this);
        _runtime.SetTargetLocation(target.GlobalPosition);
    }

    public override void OnAnimationFinish() => _runtime?.ExecuteCastTimeDone();
    public override void Stop() => _runtime?.ExecuteCancelled();
}