public interface IEnemyAttack
{
    event Action<IEntity> OnHit;
    float Effectiveness { get; }
    float Range { get; }
    float CooldownDuration { get; }
    AttackAnimation AttackAnimation { get; }

    float GetPriority();
    bool CanHit(Node3D target);
    void Execute(Node3D target);
    void Stop();
    void OnAnimationFinish();
}