[GlobalClass]
public partial class ClientBridge : Node
{

    public static ClientBridge Instance { get; private set; }
    public VFXView VFXView { get; private set; }
    public GlobePositioner GlobePositioner { get; private set; }
    public IAbilitySystem AbilitySystem { get; private set; }
    public PlayerRoot Root { get; private set; }
    public ClientPlayer ClientPlayer { get; private set; }
    public PlayerHUD PlayerHUD { get; private set; }
    public CardHandController CardHandController { get; private set; }
    public PlayerNetworkActions PlayerNetwork { get; private set; }
    public EquipmentVisuals EquipmentVisuals { get; private set; }
    public PlayerStatsSync Stats { get; private set; }

    private PassiveTreeComponent _passiveTreeComponent;

    public event Action<ClientPlayer> OnClientPlayerReady;

    public override void _Ready()
    {
        Instance = this;
        VFXView = this.GetComponentInChildren<VFXView>(true);

        var mainCamera = GetViewport().GetCamera3D();
        if (mainCamera != null)
        {
            GlobePositioner = mainCamera.GetComponentInChildren<GlobePositioner>(true);
        }
        else
        {
            GD.PushError($"[Missing Camera] No active Camera3D found in Viewport for '{Name}'.");
        }
    }

    public void RegisterLocalPlayer(ClientPlayer client)
    {
        ClientPlayer = client;

        Root = client.GetParent().GetComponentInOwner<PlayerRoot>();
        AbilitySystem = Root.GetComponentInOwner<IAbilitySystem>();
        PlayerHUD = Root.GetComponentInOwner<PlayerHUD>();
        EquipmentVisuals = Root.GetComponentInOwner<EquipmentVisuals>();
        CardHandController = GetViewport().GetCamera3D().GetComponentInChildren<CardHandController>();
        PlayerNetwork = client.GetComponentInOwner<PlayerNetworkActions>();
        Stats = client.GetComponentInOwner<PlayerStatsSync>();

        client.InitializeLocalPlayer();
        OnClientPlayerReady?.Invoke(client);
    }

}