[GlobalClass]
public partial class PassiveTooltipView : Control
{
    [Export] private Label _titleText, _roleText, _modifiersText, _costText, _requirementText;
    private Control _layerBounds;

    public override void _Ready()
    {
        _layerBounds = GetParent<Control>();
        HideTooltip();
    }

    public void Show(PassiveTooltipData data, Control anchor)
    {
        Visible = true;
        SetRow(_titleText, data.Title);
        SetRow(_roleText, data.RoleText);
        SetRow(_modifiersText, data.DescriptionText);
        SetRow(_costText, data.CostText);
        SetRow(_requirementText, data.RequirementText);
        CallDeferred(nameof(PositionNear), anchor);
    }

    public void HideTooltip() => Visible = false;

    private static void SetRow(Label field, string value)
    {
        if (field == null) return;
        field.Visible = !string.IsNullOrEmpty(value);
        if (field.Visible) field.Text = value;
    }

    private void PositionNear(Control anchor)
    {
        if (_layerBounds == null) return;
        Vector2 targetPos = anchor.GlobalPosition + new Vector2(20f, -20f);
        GlobalPosition = new Vector2(
            Mathf.Clamp(targetPos.X, 0, _layerBounds.Size.X - Size.X),
            Mathf.Clamp(targetPos.Y, 0, _layerBounds.Size.Y - Size.Y)
        );
    }
}

public struct PassiveTooltipData
{
    public string Title, RoleText, DescriptionText, CostText, RequirementText;
}