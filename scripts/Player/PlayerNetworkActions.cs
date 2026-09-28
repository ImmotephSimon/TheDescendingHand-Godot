[GlobalClass]
public partial class PlayerNetworkActions : NetworkActionBase
{
    // --- 1. Client -> Server: Item Drop ---
    public void RequestDrop(string itemId)
    {
        if (!Multiplayer.IsServer())
            RpcId(1, MethodName.RequestDropRpc, itemId);
        else
            RequestDropRpc(itemId);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestDropRpc(string itemId)
    {
        if (!Multiplayer.IsServer() || !Guid.TryParse(itemId, out var id)) return;

        var player = this.GetComponentInOwner<ServerPlayer>();
        var inventory = player.GetComponentInOwner<IInventory>();

        if (!inventory.TryGet(id, out var inventoryItem) || inventoryItem is not ItemDropInstance item) return;
        if (!inventory.TryRemove(item)) return;

        var dropNode = GameData.Instance.Items.ItemPrefab.Instantiate<ItemDrop>();
        dropNode.Initialize(item.BaseType, item.Rarity);
        dropNode.Instance = item;
        dropNode.GlobalPosition = player.GlobalPosition;

        NetworkManager.Spawn(dropNode);
    }

    // --- 2. Client -> Server: Interaction ---
    public void RequestInteract()
    {
        if (!Multiplayer.IsServer())
            RpcId(1, MethodName.RequestInteractRpc);
        else
            RequestInteractRpc();
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer)]
    private void RequestInteractRpc()
    {
        if (!Multiplayer.IsServer()) return;

        var player = this.GetComponentInOwner<ServerPlayer>();
        player.FindNearbyInteractable()?.Interact(player);
    }

    // --- 3. Server -> Target Client: Speech Bubble ---
    public void ShowSpeechBubble(string text, float duration = -1f)
    {
        if (!Multiplayer.IsServer()) return;

        var player = this.GetComponentInOwner<ServerPlayer>();
        RpcId(player.PeerId, MethodName.ShowSpeechBubbleRpc, text, duration);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void ShowSpeechBubbleRpc(string text, float duration)
    {
        ClientBridge.Instance.PlayerHUD.Hud.ShowSpeechBubble(text, duration);
    }
}