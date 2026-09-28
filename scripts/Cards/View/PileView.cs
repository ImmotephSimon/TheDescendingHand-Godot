[GlobalClass]
public partial class PileView : Control
{
    [Export] private PackedScene _cardTooltipPrefab;
    [Export] private Control _cardContainer; // e.g. HBoxContainer or CenterContainer child

    private readonly List<CardView> _views = new();

    public void Show(IReadOnlyList<CardDefinition> cardDefinitions)
    {
        Clear();

        if (_cardTooltipPrefab == null) return;
        Control parent = _cardContainer ?? this;

        for (int i = 0; i < cardDefinitions.Count; i++)
        {
            CardView tooltip = _cardTooltipPrefab.Instantiate<CardView>();
            parent.AddChild(tooltip);

            tooltip.InitializeTooltipCard(cardDefinitions[i]);
            _views.Add(tooltip);
        }
    }

    public void Clear()
    {
        foreach (var view in _views)
        {
            if (GodotObject.IsInstanceValid(view))
                view.QueueFree();
        }

        _views.Clear();
    }
}