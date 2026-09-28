[GlobalClass]
public partial class PlayerHUD : Node
{
    [Export] private PackedScene hudPrefab;
    private InventoryView _inventoryView;
    private LoadoutView _loadoutView;
    private PassiveTreeView _passiveTreeView;

    public HUD Hud { get; private set; }
    public CursorItem CursorItem { get; private set; }

    public override void _Ready()
    {
        if (hudPrefab == null)
            GD.PrintErr($"{nameof(hudPrefab)} is not assigned.");
    }

    internal void Bind(PlayerStatsSync synchedStats, PlayerItemsSync synchedItems)
    {
        var instance = hudPrefab.Instantiate();
        AddChild(instance);
        Hud = instance.GetComponentInChildren<HUD>();

        _inventoryView = Hud.GetComponentInChildren<InventoryView>(true);
        _loadoutView = Hud.GetComponentInChildren<LoadoutView>(true);
        _passiveTreeView = Hud.GetComponentInChildren<PassiveTreeView>(true);

        // Defer binding logic until HUD and synced nodes finish their _Ready setup
        Callable.From(() => CompleteBind(synchedStats, synchedItems)).CallDeferred();
    }

    private void CompleteBind(PlayerStatsSync synchedStats, PlayerItemsSync synchedItems)
    {
        var levelComp = synchedStats.GetComponentInOwner<LevelComponent>();
        var healthComp = synchedStats.GetComponentInOwner<IHealth>();
        var manaComp = synchedStats.GetComponentInOwner<IMana>();

        if (levelComp == null)
            GD.PrintErr($"[PlayerHUD] Missing {nameof(LevelComponent)} on {synchedStats.Name}");

        Hud.Bind(levelComp);
        Hud.Bind(healthComp);
        Hud.Bind(manaComp);

        _inventoryView?.Bind(synchedItems);
        _loadoutView?.Bind(synchedItems);

        ClientBridge.Instance.OnClientPlayerReady += OnClientPlayerReady;
    }

    private void OnClientPlayerReady(ClientPlayer _)
    {
        ClientBridge.Instance.OnClientPlayerReady -= OnClientPlayerReady;
        _inventoryView?.Initialize();
    }

    public void ToggleInventory()
    {
        _inventoryView?.ToggleVisibility();
    }

    public void TogglePassiveTree()
    {
        if (_passiveTreeView != null)
            _passiveTreeView.SetVisible(!_passiveTreeView.IsTreeVisible);
    }

    public IHealthBar BindBossHealthBar(IEntity enemy)
    {
        BossHealthBar bar = Hud.GetComponentInChildren<BossHealthBar>(true);
        if (bar == null) return null;

        bar.Visible = true;
        bar.Bind(enemy.Node.GetComponentInOwner<IHealth>());
        return bar;
    }

    public void UnbindBossHealthBar(IEntity enemy)
    {
        enemy.Died -= UnbindBossHealthBar;
        BossHealthBar bar = Hud.GetComponentInChildren<BossHealthBar>(true);
        if (bar == null) return;

        bar.Visible = false;
        bar.Unbind();
    }
}