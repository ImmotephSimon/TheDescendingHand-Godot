using System;

public class ChaseAction : EnemyActionBase
{
    public ChaseAction(Enemy owner) : base(owner)
    {
    }

    public override bool IsAvailable()
    {
        return perception.HasTarget;
    }

    public override void StartAction()
    {
        animationHandler.SetAnimationState(CharacterAnimationState.Locomotion);
    }

    public override void UpdateAction(float delta)
    {
        if (!perception.HasTarget)
            return;

        var target = perception.Target;

        movementHandler.RotateTowardsTarget(target);

        if (movementHandler.IsWithinStoppingDistance(target.GlobalPosition))
        {
            movementHandler.StopMovement();
            return;
        }

        movementHandler.MoveTo(target.GlobalPosition);
    }

    public override void StopAction()
    {
        movementHandler.StopMovement();
    }

    public override float GetPriority()
    {
        return 50f;
    }
}