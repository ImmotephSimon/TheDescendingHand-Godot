public interface IAnimationHandler
{
    void SetAnimationState(CharacterAnimationState state);
    void PlayAttackAnimation(AttackAnimation attackAnimation, Action onFinished);
    Action PlayAnimation(Animation clip, float duration);
    void StopCurrentAnimation();
    void SetSpeed(float clientSmoothSpeed);
}