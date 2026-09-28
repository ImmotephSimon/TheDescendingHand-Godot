using Godot;

[GlobalClass]
public partial class TooltipController : Control
{
    public static TooltipController Instance { get; private set; }

    [Export] private PackedScene itemTooltipPrefab;
    [Export] private PackedScene cardTooltipPrefab;
    [Export] private Control tooltipLayer;
    [Export] private Control fixedTooltipAnchor;

    private ItemTooltipView itemTooltip;
    private CardView cardTooltip;

    public override void _Ready()
    {
        if (Instance != null && Instance != this) { QueueFree(); return; }
        Instance = this;

        if (itemTooltipPrefab != null && tooltipLayer != null)
        {
            itemTooltip = itemTooltipPrefab.Instantiate<ItemTooltipView>();
            tooltipLayer.AddChild(itemTooltip);
            itemTooltip.GetComponentInOwner<Control>()?.Hide();
        }

        if (cardTooltipPrefab != null && tooltipLayer != null)
        {
            cardTooltip = cardTooltipPrefab.Instantiate<CardView>();
            tooltipLayer.AddChild(cardTooltip);
            cardTooltip.GetComponentInOwner<Control>()?.Hide();
        }
    }

    public void ShowItem(ItemTooltipDto item)
    {
        HideTooltip();
        if (itemTooltip == null) return;
        itemTooltip.SetItem(item);
        Show(itemTooltip.GetComponentInOwner<Control>());
    }

    public void ShowCard(CardDropInstance card)
    {
        HideTooltip();
        if (cardTooltip == null) return;
        cardTooltip.InitializeTooltipCard(card.Definition);
        Show(cardTooltip.GetComponentInOwner<Control>());
    }

    private void Show(Control control)
    {
        if (control == null) return;
        if (fixedTooltipAnchor != null)
            control.GlobalPosition = fixedTooltipAnchor.GlobalPosition;
        control.Show();
    }

    public void HideTooltip()
    {
        itemTooltip?.GetComponentInOwner<Control>()?.Hide();
        cardTooltip?.GetComponentInOwner<Control>()?.Hide();
    }
}