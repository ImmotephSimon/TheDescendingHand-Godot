[GlobalClass]
public abstract partial class WorldDrop : Area3D, IInteractable
{
    [Export] public int DropLevel { get; set; } = 1;

    public void Interact(ServerPlayer player)
    {
        if (Multiplayer.IsServer()) ExecutePickup(player);
        else RpcId(1, nameof(RequestPickupServer));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    private void RequestPickupServer()
    {
        long senderId = Multiplayer.GetRemoteSenderId();
        var player = GameWorld.Instance.GetPlayerForPeer(senderId);

        if (player == null || player.GlobalPosition.DistanceTo(GlobalPosition) > player.InteractRange)
            return;

        ExecutePickup(player);
    }

    private void ExecutePickup(ServerPlayer player)
    {
        if (TryPickup(player))
        {
            QueueFree();
        }
    }

    protected virtual bool TryPickup(ServerPlayer player) => false;
}