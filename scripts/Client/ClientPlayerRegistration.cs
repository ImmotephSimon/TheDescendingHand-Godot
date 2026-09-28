[GlobalClass]
public partial class ClientPlayerRegistration : Node3D
{
    private ClientPlayer _client;

    public override void _Ready()
    {
        if (!Multiplayer.HasMultiplayerPeer()) return;

        GD.Print($"CLIENT REGISTRATION: {Name}, IsAuthority={IsMultiplayerAuthority()}");

        _client = this.GetComponentInOwner<ClientPlayer>();

        var animHandler = this.GetComponentInChildren<IAnimationHandler>();
        var cardController = this.GetComponentInOwner<CardController>();

        cardController?.InitializeClientObservers(animHandler);

        if (IsMultiplayerAuthority())
        {
            ClientBridge.Instance.RegisterLocalPlayer(_client);
            cardController?.RpcId(1, nameof(CardController.NotifyClientReadyServerRpc));
        }
    }
}