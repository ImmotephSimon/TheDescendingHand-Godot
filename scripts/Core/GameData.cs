[GlobalClass]
public partial class GameData : Node
{
    public static GameData Instance { get; private set; }

    [Export] public CardRegistry Cards { get; private set; }
    [Export] public ItemRegistry Items { get; private set; }
    [Export] public ServerPrefabRegistry ServerPrefabs { get; private set; }
    [Export] public VfxRegistry Vfx { get; private set; }


    /*
     
    [Export] private CardRegistry cardRegistry;
    [Export] private CardRegistry testCardRegistry;

    public CardRegistry CardRegistry =>
        testCardRegistry != null && testCardRegistry.Cards.Count > 0
            ? testCardRegistry
            : cardRegistry;
     */


    public override void _EnterTree()
    {
        Instance = this;
        Items.Initialize();
        Cards.Initialize();
        ServerPrefabs.Initialize();
        Vfx.Initialize();
    }
}