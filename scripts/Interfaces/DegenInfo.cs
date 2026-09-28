using System;
using System.Collections.Generic;


public class DegenInfo
{
    public Guid Id { get; }
    public Dictionary<GameTag, float> Damage { get; set; }
    public IEntity Source { get; }
    public Vector3 Position { get; }
    public float Duration { get; }
    public int MaxStacks { get; }

    public DegenInfo(
        Guid id,
        Dictionary<GameTag, float> damage,
        IEntity source,
        Vector3 position,
        float duration,
        int maxStacks)
    {
        Id = id;
        Damage = damage;
        Source = source;
        Position = position;
        Duration = duration;
        MaxStacks = maxStacks;
    }
}