[GlobalClass]
public partial class ItemDrop : WorldDrop
{
    private Guid _definitionId;
    private static readonly Random Seed = new Random();
    private ItemDefinition _definition;
    public ItemDropInstance Instance { get; set; }
    public ItemDefinition Definition => _definition;

    public void Initialize(ItemDefinition definition, Rarity rarity)
    {
        int misfortune = Mathf.Clamp(20 - DropLevel / 10, 1, 999);
        Instance = new ItemDropInstance(definition, rarity, AffixGenerator.Generate(definition, rarity, Seed, misfortune));
        Rpc(nameof(SyncAndApplyDrop), definition.Id.ToString(), rarity.DisplayColor, rarity.LightIntensity, rarity.LightRange);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void SyncAndApplyDrop(string idStr, Color color, float intensity, float range)
    {
        if (!Guid.TryParse(idStr, out var next) || next == Guid.Empty) return;
        _definitionId = next;
        if (!GameData.Instance.Items.TryGetDefinition(next, out var def)) return;
        _definition = def;

        if (this.TryGetComponentInChildren<CollisionShape3D>(out var col) && col.Shape is BoxShape3D box)
            box.Size = 0.3f * new Vector3(def.InventorySize.X, def.InventorySize.Y, 0.5f);

        if (def.Appearance?.WorldModel != null)
            AddChild(def.Appearance.WorldModel.Instantiate() as Node3D);
        else
            GD.PushError($"Item '{def.Id}' has no WorldModel assigned.");

        if (this.TryGetComponentInChildren<OmniLight3D>(out var light))
        {
            light.LightColor = color;
            light.LightEnergy = intensity;
            light.OmniRange = range;
        }
    }

    protected override bool TryPickup(ServerPlayer player) =>
        player.TryGetComponent<IInventory>(out var inv) && inv.TryAdd(Instance);
}