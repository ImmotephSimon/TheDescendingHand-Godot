[GlobalClass]
public partial class CardRegistry : Resource
{
    [Export] private Godot.Collections.Array<CardDefinition> cards = new();
    private Dictionary<Guid, CardDefinition> _lookup;

    public Godot.Collections.Array<CardDefinition> Cards => cards;

    public bool TryGet(Guid definitionId, out CardDefinition definition)
    {
        definition = null;
        if (definitionId == Guid.Empty) return false;
        return _lookup.TryGetValue(definitionId, out definition);
    }

    public void Initialize()
    {
        if (_lookup != null) return;
        _lookup = new Dictionary<Guid, CardDefinition>();

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            if (card == null)
            {
                GD.PushWarning($"[{ResourceName}] Null entry found in Cards list at index {i}.");
                continue;
            }
            if (card.Id == Guid.Empty)
            {
                GD.PrintErr($"[{ResourceName}] CardDefinition '{card.ResourceName}' has no assigned Id.");
                continue;
            }
            if (!_lookup.TryAdd(card.Id, card))
            {
                GD.PrintErr($"[{ResourceName}] Duplicate CardDefinition Id '{card.Id}' on asset '{card.ResourceName}'. Collision with '{_lookup[card.Id].ResourceName}'.");
            }
        }
    }

    public CardDefinition GetRandomCard()
    {
        if (cards == null || cards.Count == 0) return null;
        return cards[(int)GD.RandRange(0, cards.Count - 1)];
    }
}