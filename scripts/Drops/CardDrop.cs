[GlobalClass]
public partial class CardDrop : WorldDrop
{
    private CardDefinition _definition;
    public CardItemInstance Instance { get; private set; }

    public void Initialize(CardDefinition definition)
    {
        _definition = definition;
        Instance = new CardItemInstance(definition);
        ApplyRarityVisualsRpc(new Color(0.6f, 0.1f, 1f), 2f, 5f);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void ApplyRarityVisualsRpc(Color color, float intensity, float range)
    {
        uint layer1 = 1u << 1;

        var light = this.GetComponentInChildren<Light3D>();
        if (light != null)
        {
            light.LightColor = color;
            light.LightEnergy = intensity;
            if (light is OmniLight3D omni) omni.OmniRange = range;
            light.LightCullMask = layer1;
        }
    }

    protected override bool TryPickup(ServerPlayer player)
    {
        return player.GetComponentInOwner<IInventory>().TryAdd(Instance);
    }
}