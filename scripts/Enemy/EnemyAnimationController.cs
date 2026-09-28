using Godot;
using System;
using System.Threading.Tasks;

[GlobalClass]
public partial class EnemyAnimationController : Node3D, IAnimationHandler
{
    private const float TransitionDuration = 0.1f;

    private AnimationPlayer _animator;
    private CharacterAnimationState _currentState;
    private Action _attackFinished;
    private AttackHitbox _hitbox;
    private bool _isAttacking;

    public override void _Ready()
    {
        _animator = this.GetComponentInChildren<AnimationPlayer>();
        _hitbox = this.GetComponentInChildren<AttackHitbox>(required: false);
    }

    public void HitboxEnable() => _hitbox?.Enable();
    public void HitboxDisable() => _hitbox?.Disable();

    public void SetSpeed(float speed)
    {
        if (_animator != null) _animator.SpeedScale = speed;
    }

    public void SetAnimationState(CharacterAnimationState state) => PlayState(state);

    public async void PlayAttackAnimation(AttackAnimation attackAnimation, Action onFinished)
    {
        StopCurrentAnimation();
        _currentState = CharacterAnimationState.Unset;

        string animName = attackAnimation switch
        {
            AttackAnimation.MeleeFast => "FastAttack",
            AttackAnimation.MeleeSlow => "SlowAttack",
            AttackAnimation.Charge => "ChargeAttack",
            AttackAnimation.Special => "SpecialAttack",
            _ => throw new ArgumentOutOfRangeException(nameof(attackAnimation))
        };

        _attackFinished = onFinished;
        _isAttacking = true;

        _animator?.Play(animName, customBlend: TransitionDuration);

        float length = (float?)_animator?.GetAnimation(animName)?.Length ?? 1f;
        await Task.Delay(TimeSpan.FromSeconds(length));

        if (!_isAttacking) return;

        SetAnimationState(CharacterAnimationState.Locomotion);
        _isAttacking = false;
        var finished = _attackFinished;
        _attackFinished = null;
        finished?.Invoke();
    }

    public void StopCurrentAnimation()
    {
        _isAttacking = false;
        _hitbox?.Disable();
        _attackFinished = null;
    }

    public void PlayState(CharacterAnimationState state, float transitionDuration = TransitionDuration)
    {
        if (state == CharacterAnimationState.Unset || _currentState == state || _isAttacking)
            return;

        _currentState = state;
        string animName = state switch
        {
            CharacterAnimationState.Locomotion => "EnemyLocomotion",
            CharacterAnimationState.Immobilized => "Stunned",
            CharacterAnimationState.Dead => "Dead",
            _ => throw new ArgumentOutOfRangeException(nameof(state))
        };

        _animator?.Play(animName, customBlend: transitionDuration);
    }

    public Action PlayAnimation(Animation clip, float duration) => null;
}