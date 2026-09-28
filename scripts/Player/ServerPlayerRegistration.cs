[GlobalClass]
public partial class ServerPlayerRegistration : Node
{
    private ServerPlayer player;

    public override void _Ready()
    {
        if (!Multiplayer.IsServer())
            return;

        player = this.GetComponentInOwner<ServerPlayer>();

        if (GameWorld.Instance == null)
            throw new System.NullReferenceException("GameWorld missing.");

        GameWorld.Instance.RegisterEntity(player);

        var cardController = this.GetComponentInOwner<CardController>();
        if (cardController == null)
            throw new System.NullReferenceException($"[{Name}] Missing CardController.");

        var networkActions = this.GetComponentInOwner<PlayerNetworkActions>();

        cardController.InitializeServer(player);
    }
}