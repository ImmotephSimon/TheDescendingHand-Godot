[GlobalClass]
public partial class CardView : Control
{
    [Export] private RichTextLabel _title;
    [Export] private RichTextLabel _description;
    [Export] private RichTextLabel _keybind;
    [Export] private TextureRect _art;

    private CardDefinition _definition;

    public void InitializeTooltipCard(CardDefinition definition)
    {
        _definition = definition;
        if (definition?.Visuals != null)
            ApplyVisuals(definition.Visuals);
    }

    public void SetKeybindText(string keybindText)
    {
        if (_keybind != null)
            _keybind.Text = keybindText ?? string.Empty;
    }

    public void ApplyVisuals(CardVisuals visuals)
    {
        if (_title != null) _title.Text = visuals.Name;
        if (_description != null) _description.Text = visuals.Description;

        if (_art != null && visuals.Icon != null)
            _art.Texture = visuals.Icon;
    }
}