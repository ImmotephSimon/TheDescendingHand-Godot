using System;


public class CardInstance
{
    public IEntity Owner => _owner;

    public Guid Id { get; }
    public CardDefinition Definition { get; }

    public float CastTime => Definition.CastTime;
    public bool SpawnAtCursor => Definition.SpawnAtCursor;

    private readonly IEntity _owner;

    public CardInstance(
        Guid id,
        CardDefinition definition,
        IEntity owner)
    {
        Id = id;
        Definition = definition;
        _owner = owner;
    }
}