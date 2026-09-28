[GlobalClass]
public partial class CardHandView : Node3D
{
    [Export] private float spacing = 0.15f;
    [Export] private float fanAngle = 30f;
    [Export] private float moveSpeed = 12f;

    private readonly List<CardHandFollower> cards = new();

    public override void _Process(double delta)
    {
        UpdateLayout();
        MoveCards((float)delta);
    }

    private void UpdateLayout()
    {
        if (cards.Count == 0) return;

        int count = cards.Count;

        for (int i = 0; i < count; i++)
        {
            float centerOffset = i - (count - 1) * 0.5f;

            Vector3 position = new(
                centerOffset * spacing,
                0,
                i * -0.02f
            );

            Quaternion rotation = Quaternion.FromEuler(new Vector3(
                0,
                0,
                Mathf.DegToRad(centerOffset * -fanAngle)
            ));

            cards[i].SetHandTransform(position, rotation);
        }
    }

    private void MoveCards(float delta)
    {
        foreach (var card in cards)
            card.MoveTowardsHandTransform(moveSpeed, delta);
    }

    public void AddCard(Node physicalCard)
    {
        if (physicalCard == null) return;

        CardHandFollower follower = physicalCard.GetComponentInChildren<CardHandFollower>();
        if (follower == null) return;

        cards.Add(follower);
        follower.UpdateParent(this);
        UpdateLayout();
    }

    public void RemoveCard(CardHandFollower card)
    {
        if (card == null) return;

        cards.Remove(card);
        card.DestroyCard();
        UpdateLayout();
    }
}