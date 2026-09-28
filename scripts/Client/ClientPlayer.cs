[GlobalClass]
public partial class ClientPlayer : Node
{
    private PlayerMovementController _playerMovement;
    private PlayerStatsSync _synchedStats;
    private PlayerItemsSync _synchedItems;

    public Vector3 CursorPosition => _playerMovement.CursorPosition;

    public override void _EnterTree()
    {
        base._EnterTree();

        Owner = (Node)this.GetParent().GetComponentInOwner<PlayerRoot>();
        _playerMovement = this.GetComponentInOwner<PlayerMovementController>();
        _synchedStats = this.GetComponentInOwner<PlayerStatsSync>();
        _synchedItems = this.GetComponentInOwner<PlayerItemsSync>();

        Debug.Assert(_synchedStats != null, $"{Name} missing PlayerStatsSync");
        Debug.Assert(_synchedItems != null, $"{Name} missing PlayerItemsSync");
    }

    public void InitializeLocalPlayer()
    {
        ClientBridge.Instance.PlayerHUD.Bind(_synchedStats, _synchedItems);
        ClientBridge.Instance.GlobePositioner.Initialize(_synchedStats);
    }

    public void TryInteract()
    {
        GetNodeOrNull<PlayerNetworkActions>("PlayerNetworkActions")?.RequestInteract();
    }
}