[GlobalClass]
public partial class ItemRegistry : Resource
{
    [Export] private PackedScene itemPrefab;
    [Export] private Godot.Collections.Array<Rarity> rarities = new();


    [Export] private Godot.Collections.Array<ItemDefinition> items = new();
    public Godot.Collections.Array<ItemDefinition> Items => items;
    public PackedScene ItemPrefab => itemPrefab;
    public Godot.Collections.Array<Rarity> Rarities => rarities;

    private Dictionary<Guid, ItemDefinition> _lookup;

    public void Initialize()
    {
        if (_lookup != null)
            return;

        _lookup = new Dictionary<Guid, ItemDefinition>();

        foreach (var item in items)
        {
            if (item == null || item.Id == Guid.Empty)
                continue;

            if (!_lookup.TryAdd(item.Id, item))
            {
                GD.PrintErr($"[ItemRegistry] Duplicate ItemDefinition Id '{item.Id}' on '{item.ResourceName}'.");
            }
        }
    }

    public bool TryGetIcon(Guid id, out Texture2D icon)
    {
        Initialize();

        if (_lookup.TryGetValue(id, out var itemDef))
        {
            icon = itemDef.Appearance.Icon;
            return true;
        }

        if (GameData.Instance.Cards.TryGet(id, out var cardDef))
        {
            icon = cardDef.Visuals.Icon;
            return true;
        }

        GD.PrintErr($"Invalid id for TryGetInventoryItem (id: {id})");
        icon = null;
        return false;
    }

    public bool TryGetDefinition(Guid id, out ItemDefinition definition)
    {
        Initialize();
        if (!_lookup.TryGetValue(id, out definition))
        {
            GD.PrintErr($"Invalid id for TryGetDefinition (id: {id})");
            return false;
        }

        return true;
    }

    public Rarity RollRandomRarity(System.Random rng, int minimumTier = 0)
    {
        float totalWeight = 0f;

        foreach (var rarity in rarities)
        {
            if (rarity.Tier < minimumTier)
                continue;

            totalWeight += rarity.DropWeight;
        }

        if (totalWeight <= 0f)
            return null;

        double roll = rng.NextDouble() * totalWeight;

        foreach (var rarity in rarities)
        {
            if (rarity.Tier < minimumTier)
                continue;

            roll -= rarity.DropWeight;

            if (roll < 0)
                return rarity;
        }

        return null;
    }

    public bool TryGetRarity(Guid id, out Rarity rarity)
    {
        rarity = null;
        foreach (var r in rarities)
        {
            if (r != null && r.Id == id)
            {
                rarity = r;
                break;
            }
        }
        return rarity != null;
    }

    public ItemDefinition RollRandomItem()
    {
        if (items == null || items.Count == 0)
        {
            GD.PrintErr("No items set.");
            return null;
        }

        int index = (int)GD.RandRange(0, items.Count - 1);
        return items[index];
    }
}