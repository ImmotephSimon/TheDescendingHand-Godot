using GodotArray = Godot.Collections.Array;
using GodotDict = Godot.Collections.Dictionary;

[GlobalClass]
public partial class PlayerItemsSync : Node
{
    private PlayerInventory _inventory;
    private Loadout _loadout;

    private readonly List<InventoryItemDto> _inventoryItems = new();
    private readonly List<ItemTooltipDto> _equippedItems = new();

    public event Action InventoryChanged;
    public event Action LoadoutChanged;

    private int _inventoryRows = 4;
    private int _inventoryColumns = 4;

    public int InventoryRows => _inventoryRows;
    public int InventoryColumns => _inventoryColumns;
    public IReadOnlyList<InventoryItemDto> InventoryItems => _inventoryItems;
    public IReadOnlyList<ItemTooltipDto> EquippedItems => _equippedItems;

    /// <summary>Peer id that owns this player. Set by whatever spawns the player.</summary>
    public long OwnerPeerId { get; set; } = 1;

    public override void _Ready()
    {
        if (!Multiplayer.IsServer())
            return; // clients wait for snapshots

        _inventory = this.GetComponentInOwner<PlayerInventory>();
        _loadout = _inventory.Loadout;

        _inventoryRows = _inventory.Rows;
        _inventoryColumns = _inventory.Columns;

        _inventory.OnChanged += SyncInventory;
        _loadout.OnLoadoutChanged += HandleLoadoutChanged;

        SyncInventory();
        HandleLoadoutChanged();
    }

    public override void _ExitTree()
    {
        if (_inventory != null) _inventory.OnChanged -= SyncInventory;
        if (_loadout != null) _loadout.OnLoadoutChanged -= HandleLoadoutChanged;
    }

    /// <summary>Call when a new peer should receive the current state (e.g. after spawn/join).</summary>
    public void SendFullStateTo(long peerId)
    {
        if (!Multiplayer.IsServer()) return;
        RpcId(peerId, nameof(Client_ReceiveInventory), _inventoryRows, _inventoryColumns, SerializeInventory());
        RpcId(peerId, nameof(Client_ReceiveLoadout), SerializeLoadout());
    }

    // ---------------------------------------------------------------- inventory

    private void SyncInventory()
    {
        _inventoryItems.Clear();

        foreach (var entry in _inventory.GetItemPositions())
        {
            _inventoryItems.Add(new InventoryItemDto
            {
                ItemId = entry.Key.InventoryId,
                Position = entry.Value,
                Size = entry.Key.Size
            });
        }

        // Server-side listeners (host UI, etc.)
        InventoryChanged?.Invoke();

        // Replicate to the owning client (skip if the host is the owner).
        if (OwnerPeerId != Multiplayer.GetUniqueId())
            RpcId(OwnerPeerId, nameof(Client_ReceiveInventory), _inventoryRows, _inventoryColumns, SerializeInventory());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ReceiveInventory(int rows, int columns, GodotArray items)
    {
        _inventoryRows = rows;
        _inventoryColumns = columns;

        _inventoryItems.Clear();
        foreach (var v in items)
            _inventoryItems.Add(InventoryItemFromVariant(v.AsGodotDictionary()));

        InventoryChanged?.Invoke();
    }

    private GodotArray SerializeInventory()
    {
        var arr = new GodotArray();
        foreach (var dto in _inventoryItems)
        {
            arr.Add(new GodotDict
            {
                ["item_id"] = dto.ItemId.ToString(),
                ["pos"] = new Vector2I(dto.Position.X, dto.Position.Y),
                ["size"] = new Vector2I(dto.Size.X, dto.Size.Y),
            });
        }
        return arr;
    }

    private static InventoryItemDto InventoryItemFromVariant(GodotDict d)
    {
        var pos = d["pos"].AsVector2I();
        var size = d["size"].AsVector2I();
        return new InventoryItemDto
        {
            ItemId = Guid.Parse(d["item_id"].AsString()),   // adjust if ItemId isn't a Guid
            Position = new Vector2I(pos.X, pos.Y),        // your own Vector2Int type
            Size = new Vector2I(size.X, size.Y)
        };
    }

    /// <summary>Kept from the original: finds the item covering a given cell.</summary>
    public bool TryGetInventoryItem(int row, int column, out InventoryItemDto item)
    {
        foreach (var dto in _inventoryItems)
        {
            var origin = dto.Position;
            var size = dto.Size;

            if (column < origin.X || column >= origin.X + size.X ||
                row < origin.Y || row >= origin.Y + size.Y)
                continue;

            item = dto;
            return true;
        }

        item = default;
        return false;
    }

    // ---------------------------------------------------------------- click RPCs

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestInventorySlotClick(int row, int column)
    {
        if (!IsValidOwnerRequest()) return;
        _inventory.SlotLeftClicked(row, column);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void RequestInventorySlotRightClick(int row, int column)
    {
        if (!IsValidOwnerRequest()) return;
        _inventory.SlotRightClicked(row, column);
    }

    // ---------------------------------------------------------------- loadout

    private void HandleLoadoutChanged()
    {
        _equippedItems.Clear();
        foreach (var entry in _loadout.Equipped)
            _equippedItems.Add(CreateItemTooltip(entry.Value));

        LoadoutChanged?.Invoke();

        if (OwnerPeerId != Multiplayer.GetUniqueId())
            RpcId(OwnerPeerId, nameof(Client_ReceiveLoadout), SerializeLoadout());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Client_ReceiveLoadout(GodotArray equipped)
    {
        _equippedItems.Clear();
        foreach (var v in equipped)
            _equippedItems.Add(TooltipFromVariant(v.AsGodotDictionary()));

        LoadoutChanged?.Invoke();
    }

    private GodotArray SerializeLoadout()
    {
        var arr = new GodotArray();
        foreach (var dto in _equippedItems)
            arr.Add(TooltipToVariant(dto));
        return arr;
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Server_RequestUnequip(string equipmentTypeIdStr)
    {
        if (!IsValidOwnerRequest()) return;

        if (!Guid.TryParse(equipmentTypeIdStr, out Guid equipmentTypeId))
        {
            GD.PushWarning($"Unequip: bad guid '{equipmentTypeIdStr}'");
            return;
        }

        if (EquipmentType.TryGet(equipmentTypeId, out EquipmentType slot))
            _loadout.Unequip(slot);
        else
            GD.PushWarning($"Unequip: unknown equipment type {equipmentTypeId}");
    }

    // ---------------------------------------------------------------- tooltips

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    public void Server_RequestInventoryTooltip(int row, int column)
    {
        if (!IsValidOwnerRequest()) return;

        if (!_inventory.TryGet(row, column, out IInventoryItem item))
            return;

        if (item is not ItemDropInstance itemDrop)
        {
            GD.PushError("visualizing cards unhandled");
            return;
        }

        var dto = CreateItemTooltip(itemDrop);
        RpcId(Multiplayer.GetRemoteSenderId(), nameof(Target_InventoryTooltipRpc), TooltipToVariant(dto));
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void Target_InventoryTooltipRpc(GodotDict dtoData)
    {
        // Only accept from the server.
        if (Multiplayer.GetRemoteSenderId() != 1) return;

        TooltipController.Instance.ShowItem(TooltipFromVariant(dtoData));
    }

    private ItemTooltipDto CreateItemTooltip(ItemDropInstance item)
    {
        var equipUse = item.BaseType.Components
            .OfType<EquipComponentDefinition>()
            .FirstOrDefault();

        System.Diagnostics.Debug.Assert(equipUse != null, "CreateItemTooltip for item which can't be equipped.");

        return new ItemTooltipDto
        {
            EquipmentTypeId = equipUse.EquipmentType.Id,
            BaseTypeId = item.BaseType.Id,
            RarityId = item.Rarity.Id,

            Implicits = item.BaseType.Implicits
                .Select(x => x.Modifier)
                .Select(AffixState.FromModifier)
                .ToList(),

            Explicits = item.Explicits
                .Select(affix => AffixState.FromInstance(affix, item.BaseType.Implicits))
                .ToList()
        };
    }

    // ---------------------------------------------------------------- tooltip DTO <-> Variant

    private static GodotDict TooltipToVariant(ItemTooltipDto dto)
    {
        var implicits = new GodotArray();
        foreach (var a in dto.Implicits) implicits.Add(AffixToVariant(a));

        var explicits = new GodotArray();
        foreach (var a in dto.Explicits) explicits.Add(AffixToVariant(a));

        return new GodotDict
        {
            ["equipment_type"] = dto.EquipmentTypeId.ToString(),
            ["base_type"] = dto.BaseTypeId.ToString(),
            ["rarity"] = dto.RarityId.ToString(),
            ["implicits"] = implicits,
            ["explicits"] = explicits,
        };
    }

    private static ItemTooltipDto TooltipFromVariant(GodotDict d)
    {
        var dto = new ItemTooltipDto
        {
            EquipmentTypeId = Guid.Parse(d["equipment_type"].AsString()),
            BaseTypeId = Guid.Parse(d["base_type"].AsString()),
            RarityId = Guid.Parse(d["rarity"].AsString()),
            Implicits = new List<AffixState>(),
            Explicits = new List<AffixState>(),
        };

        foreach (var v in d["implicits"].AsGodotArray())
            dto.Implicits.Add(AffixFromVariant(v.AsGodotDictionary()));
        foreach (var v in d["explicits"].AsGodotArray())
            dto.Explicits.Add(AffixFromVariant(v.AsGodotDictionary()));

        return dto;
    }

    // TODO: fill in with the real fields of AffixState
    private static GodotDict AffixToVariant(AffixState a) => throw new NotImplementedException();
    private static AffixState AffixFromVariant(GodotDict d) => throw new NotImplementedException();

    // ---------------------------------------------------------------- security

    /// <summary>Mirror of FishNet's RequireOwnership = true.</summary>
    private bool IsValidOwnerRequest()
    {
        if (!Multiplayer.IsServer()) return false;

        long sender = Multiplayer.GetRemoteSenderId();
        // sender == 0 means a local (non-RPC) call, e.g. host calling directly.
        return sender == 0 || sender == OwnerPeerId;
    }
}