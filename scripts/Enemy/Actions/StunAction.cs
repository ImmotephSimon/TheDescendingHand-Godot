

public class StunAction : EnemyActionBase
{
    public StunAction(Enemy owner) : base(owner)
    {
    }

    public override bool IsAvailable()
    {
        return true;
    }

    public override float GetPriority()
    {
        return 200f;
    }

    public override void StartAction()
    {
        animationHandler.SetAnimationState(CharacterAnimationState.Immobilized);

        movementHandler.StopMovement();
    }

    public override void UpdateAction(float delta)
    {
        // Do nothing while stunned.
        // Could tick a stun timer here.
    }

    public override void StopAction()
    {
    }
}