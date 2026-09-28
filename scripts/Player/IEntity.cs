public interface IEntity
{
    bool IsDead { get; }
    Node3D Node { get; }

    event Action<IEntity> Died;

    IStatContainer Stats { get; }

    /// <summary>
    /// Target team layer for physics/queries (bitmask)
    /// </summary>
    uint HostileLayer { get; }

    /// <summary>
    /// Layer assigned to spawned damage sources (bitmask)
    /// </summary>
    uint AttackLayer { get; }

    Vector3 GetCursorPosition();
    List<(StatModifier, float)> OnHitStats { get; }

    void Die(IEntity killer);
}