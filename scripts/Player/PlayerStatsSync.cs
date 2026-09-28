[GlobalClass]
public partial class PlayerStatsSync : Node
{
    // These are Godot properties because MultiplayerSynchronizer replicates
    // Godot-visible properties, not arbitrary C# fields/properties.
    [Export] public float Health { get; private set; }
    [Export] public float MaxHealth { get; private set; }
    [Export] public float Mana { get; private set; }
    [Export] public float MaxMana { get; private set; }
    [Export] public int Level { get; private set; }

    private MultiplayerSynchronizer _synchronizer;
    private int _ownerId;
    private IStatContainer _stats;
    private IHealth _healthHandler;
    private IMana _manaHandler;
    private readonly HashSet<string> _replicatedStatuses = new();

    public event Action<GameTag, bool> StatusChanged;
    public event Action<float, float, bool> HealthChanged;
    public event Action<float, float, bool> ManaChanged;

    public HashSet<string> Statuses => _replicatedStatuses;

    public override void _EnterTree()
    {
        _ownerId = GetMultiplayerAuthority();
        SetMultiplayerAuthority(1);

        EnsureSynchronizer();

        if (!Multiplayer.IsServer())
            return;

        _healthHandler = this.GetComponentInOwner<IHealth>();
        _manaHandler = this.GetComponentInOwner<IMana>();

        if (_healthHandler != null)
            _healthHandler.OnHealthChanged += OnHealthChanged;
        if (_manaHandler != null)
            _manaHandler.OnManaChanged += OnManaChanged;

        _stats = this.GetComponentInOwner<IStatContainer>();
        if (_stats == null || GameTags.Statuses == null)
            return;

        foreach (var tag in GameTags.Statuses)
            _stats.Listen(tag, _ => OnStatusChanged(tag));
    }

    private void EnsureSynchronizer()
    {
        _synchronizer = GetNodeOrNull<MultiplayerSynchronizer>("StatsSynchronizer");
        if (_synchronizer != null)
            return;

        _synchronizer = new MultiplayerSynchronizer
        {
            Name = "StatsSynchronizer",
            RootPath = new NodePath("..")
        };

        var config = new SceneReplicationConfig();
        AddReplicatedProperty(config, nameof(Health));
        AddReplicatedProperty(config, nameof(MaxHealth));
        AddReplicatedProperty(config, nameof(Mana));
        AddReplicatedProperty(config, nameof(MaxMana));
        AddReplicatedProperty(config, nameof(Level));

        _synchronizer.ReplicationConfig = config;
        AddChild(_synchronizer);
    }

    private static void AddReplicatedProperty(SceneReplicationConfig config, string propertyName)
    {
        var path = new NodePath(propertyName);
        config.AddProperty(path);
        config.PropertySetReplicationMode(
            path,
            SceneReplicationConfig.ReplicationMode.OnChange);
        config.PropertySetSpawn(path, true);
    }

    private void OnHealthChanged(float current, float max, bool isInstant)
    {
        if (!Multiplayer.IsServer())
            return;

        Health = current;
        MaxHealth = max;

        SendHealthChanged(current, max, isInstant);
    }

    private void OnManaChanged(float current, float max, bool isInstant)
    {
        if (!Multiplayer.IsServer())
            return;

        Mana = current;
        MaxMana = max;

        SendManaChanged(current, max, isInstant);
    }

    private void SendHealthChanged(float current, float max, bool isInstant)
    {
        int ownerId = _ownerId;
        if (ownerId <= 0)
            return;

        if (ownerId == Multiplayer.GetUniqueId())
            HealthChanged?.Invoke(current, max, isInstant);
        else
            RpcId(ownerId, nameof(HealthChangedRpc), current, max, isInstant);
    }

    private void SendManaChanged(float current, float max, bool isInstant)
    {
        int ownerId = _ownerId;
        if (ownerId <= 0)
            return;

        if (ownerId == Multiplayer.GetUniqueId())
            ManaChanged?.Invoke(current, max, isInstant);
        else
            RpcId(ownerId, nameof(ManaChangedRpc), current, max, isInstant);
    }

    private void OnStatusChanged(GameTag tag)
    {
        if (!Multiplayer.IsServer() || _stats == null)
            return;

        string tagId = tag.TagId;
        float value = _stats.GetStat(tag);

        // Godot RPCs are remote by default, so apply it locally for a host/server too.
        ApplyStatus(tagId, value);
        Rpc(nameof(SyncStatusRpc), tagId, value);
    }

    [Rpc(
        MultiplayerApi.RpcMode.Authority,
        CallLocal = false,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void HealthChangedRpc(float current, float max, bool isInstant)
    {
        if (Multiplayer.GetRemoteSenderId() != 1)
            return;

        HealthChanged?.Invoke(current, max, isInstant);
    }

    [Rpc(
        MultiplayerApi.RpcMode.Authority,
        CallLocal = false,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void ManaChangedRpc(float current, float max, bool isInstant)
    {
        if (Multiplayer.GetRemoteSenderId() != 1)
            return;

        ManaChanged?.Invoke(current, max, isInstant);
    }

    [Rpc(
        MultiplayerApi.RpcMode.Authority,
        CallLocal = false,
        TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SyncStatusRpc(string tagId, float value)
    {
        ApplyStatus(tagId, value);
    }

    private void ApplyStatus(string tagId, float value)
    {
        bool isActive = value > 0f;

        if (isActive)
            _replicatedStatuses.Add(tagId);
        else
            _replicatedStatuses.Remove(tagId);

        if (GameTags.Statuses == null)
            return;

        foreach (var tag in GameTags.Statuses)
        {
            if (tag.TagId != tagId)
                continue;

            StatusChanged?.Invoke(tag, isActive);
            break;
        }
    }

    public override void _ExitTree()
    {
        if (_healthHandler != null)
            _healthHandler.OnHealthChanged -= OnHealthChanged;
        if (_manaHandler != null)
            _manaHandler.OnManaChanged -= OnManaChanged;
    }
}
