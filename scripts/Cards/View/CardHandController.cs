[GlobalClass]
public partial class CardHandController : Node
{
    [Export] private CardHandView handView;
    [Export] private PackedScene physicalCardPrefab;

    private Node[] cards = new Node[10];

    public void OnCardAdded(int index, Guid cardDefinitionId)
    {
        if (physicalCardPrefab == null || handView == null) return;

        Node cardObject = physicalCardPrefab.Instantiate();
        handView.AddChild(cardObject);

        CardHandFollower follower = cardObject.GetComponentInChildren<CardHandFollower>();
        GameData.Instance.Cards.TryGet(cardDefinitionId, out CardDefinition def);
        follower?.InitializePhysicalCard(def, cardObject as Node3D, $"{index + 1}");

        cards[index] = cardObject;
        handView.AddCard(cardObject);
    }

    public void OnCardRemoved(int index)
    {
        if (cards[index] == null) return;

        CardHandFollower follower = cards[index].GetComponentInChildren<CardHandFollower>();
        if (handView != null && follower != null)
            handView.RemoveCard(follower);

        cards[index].QueueFree();
        cards[index] = null;
    }
}