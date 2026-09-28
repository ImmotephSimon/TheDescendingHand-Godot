using System.Text.RegularExpressions;



[Tool]
[GlobalClass]
public partial class ItemDefinition : Resource
{
    public Guid Id;
    [Export] public string DisplayName;
    [Export] public Vector2I InventorySize = new(2, 2);
    [Export] public AppearanceData Appearance;
    [Export] public int RequiredLevel;
    [Export(PropertyHint.MultilineText)] public string Lore;
    [Export] public TagContainer Tags;
    public List<ItemImplicit> Implicits = new();
    public List<ItemComponentDefinition> Components = new();

    public override void _Notification(int what)
    {
        if (Engine.IsEditorHint() && what == (int)NotificationPostinitialize && !string.IsNullOrEmpty(ResourcePath))
        {
            if (Id == Guid.Empty) Id = Guid.NewGuid();
            if (string.IsNullOrEmpty(DisplayName)) DisplayName = DisplayFormat(ResourcePath.GetFile().GetBaseName());
        }
    }

    public static string DisplayFormat(string input) => Regex.Replace(input, "([a-z0-9])([A-Z])", "$1 $2");
}