
[GlobalClass]
public partial class PlayerRoot : CharacterBody3D
{
    [Export] private PackedScene _serverPlayerScene;
    [Export] private PackedScene _clientPlayerScene;

    private ClientPlayer _client;

    public override void _EnterTree()
    {
        GD.Print($"[PlayerRoot:{Name}] display={DisplayServer.GetName()} " +
         $"hasPeer={Multiplayer.HasMultiplayerPeer()} " +
         $"isServer={Multiplayer.IsServer()} " +
         $"uniqueId={Multiplayer.GetUniqueId()} " +
         $"headless={IsDedicatedServer()}");


        // Spawn only. Authority may not be assigned yet, so don't make
        // authority decisions here (see notes below).
        if (Multiplayer.IsServer())
            SpawnPlayerScene(_serverPlayerScene, "ServerPlayer");

        // Dedicated server has no local player, everyone else gets a client scene.
        // Remove the DisplayServer check if you have no headless builds.
        if (!IsDedicatedServer())
            SpawnPlayerScene(_clientPlayerScene, "ClientPlayer");
    }

    public override void _Ready()
    {
        if (!Multiplayer.HasMultiplayerPeer()) return;

        // Server-side registration logic
        if (Multiplayer.IsServer())
        {
            var serverPlayer = this.GetComponentInOwner<ServerPlayer>();

            if (GameWorld.Instance == null)
                throw new System.NullReferenceException($"[{Name}] GameWorld missing.");

            GameWorld.Instance.RegisterEntity(serverPlayer);

            var serverCardController = this.GetComponentInOwner<CardController>();
            if (serverCardController == null)
                throw new System.NullReferenceException($"[{Name}] Missing CardController.");

            var networkActions = this.GetComponentInOwner<PlayerNetworkActions>();

            serverCardController.InitializeServer(serverPlayer);
        }

        _client = this.GetComponentInChildren<ClientPlayer>();
        var animHandler = this.GetComponentInChildren<IAnimationHandler>();
        var cardController = this.GetComponentInOwner<CardController>();

        // Observers wire up for every client-side instance (local + remote views)
        cardController?.InitializeClientObservers(animHandler);

        // Only the owning peer registers as the local player
        if (IsMultiplayerAuthority())
        {
            ClientBridge.Instance.RegisterLocalPlayer(_client);
            cardController?.RpcId(1, nameof(CardController.NotifyClientReadyServerRpc));

            var camera = GetViewport().GetCamera3D() as ARPGCamera;
            camera.SetPlayer(this);
        }
    }

    private void SpawnPlayerScene(PackedScene scene, string nodeName)
    {
        if (scene == null)
        {
            GD.PushError($"[PlayerRoot] No scene assigned for '{nodeName}' on '{Name}'");
            return;
        }

        var instance = scene.Instantiate();
        instance.Name = nodeName;        
        AddChild(instance);

    }

    private static bool IsDedicatedServer() =>
        DisplayServer.GetName() == "headless";
}