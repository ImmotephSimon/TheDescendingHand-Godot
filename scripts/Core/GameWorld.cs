[GlobalClass]
public partial class GameWorld : Node3D
{
    public static GameWorld Instance { get; private set; }

    private readonly List<IEntity> _entities = new();
    private readonly Dictionary<long, ServerPlayer> _playerRegistry = new();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree() { if (Instance == this) Instance = null; }

    public void RegisterPlayer(long peerId, ServerPlayer player) => _playerRegistry[peerId] = player;
    public void UnregisterPlayer(long peerId) => _playerRegistry.Remove(peerId);
    public ServerPlayer GetPlayerForPeer(long peerId) => _playerRegistry.TryGetValue(peerId, out var p) ? p : null;

    public void RegisterEntity(IEntity entity) { if (!_entities.Contains(entity)) _entities.Add(entity); }
    public void UnregisterEntity(IEntity entity) => _entities.Remove(entity);
    public void NotifyDeath(IEntity victim, IEntity killer) => EntityDied?.Invoke(victim, killer);
    public void NotifyRevive(IEntity entity) => EntityRevived?.Invoke(entity);
    public IReadOnlyList<IEntity> GetEntities() => _entities;

    public event Action<IEntity> EntityRevived;
    public event Action<IEntity, IEntity> EntityDied;
}