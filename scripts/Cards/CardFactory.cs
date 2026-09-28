public static class CardFactory
{
    private static PackedScene _cardRuntimePrefab;

    public static void Initialize(
        CardRegistry registry,
        PackedScene cardRuntimePrefab)
    {
        _cardRuntimePrefab = cardRuntimePrefab;
    }

    public static CardInstance CreateCardInstance(CardDefinition definition, IEntity owner)
        => new(Guid.NewGuid(), definition, owner);

    public static CardRuntime CreateRuntime(CardInstance card, Node context)
    {
        if (!context.Multiplayer.IsServer())
        {
            GD.PushError("CardFactory.CreateRuntime can only be called on the server.");
            return null;
        }

        if (_cardRuntimePrefab == null || _cardRuntimePrefab.Instantiate() is not CardRuntime runtime)
        {
            GD.PushError("CardFactory missing valid CardRuntime scene/prefab.");
            return null;
        }

        runtime.Initialize(Guid.NewGuid(), card.Definition, card.Owner);

        card.Definition.Construct(
            new CardInitContext(
                Guid.NewGuid(),
                card.Owner),
            runtime);

        return runtime;
    }

}