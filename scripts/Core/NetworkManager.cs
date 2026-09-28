[GlobalClass]
public partial class NetworkManager : Node
{
    public static NetworkManager Instance { get; private set; }
    private Node _spawnContainer;

    private const int MaxSpawnedObjects = 50;
    private static readonly Queue<Node> SpawnedObjects = new();
    private readonly Dictionary<Guid, Node> _activeClientVfx = new();

    public override void _Ready()
    {
        Instance = this;
        _spawnContainer = this.GetComponentInChildren<Node3D>();
    }

    // --- 1. Capped Server Spawning ---
    public static T Spawn<T>(T node) where T : Node
    {
        if (Instance == null || !Instance.Multiplayer.IsServer()) return node;

        Instance._spawnContainer.AddChild(node, forceReadableName: true);
        SpawnedObjects.Enqueue(node);

        while (SpawnedObjects.Count > MaxSpawnedObjects)
        {
            var oldest = SpawnedObjects.Dequeue();
            if (GodotObject.IsInstanceValid(oldest) && oldest.IsInsideTree())
            {
                oldest.QueueFree();
            }
        }

        return node;
    }

    // --- 2. Persistent Client VFX Handling ---
    public static Action SpawnClientVfx(Guid cardId, VfxSpawnParams vfxParams)
    {
        if (Instance == null || !Instance.Multiplayer.IsServer())
            return () => { };

        Instance.Rpc(MethodName.RpcSpawnClientVfx, cardId.ToString(), vfxParams.InstanceId.ToString(), vfxParams.VfxIndex, vfxParams.Position, vfxParams.Rotation, vfxParams.Scale, vfxParams.Attach);

        // Return closure to stop VFX later across all clients
        return () => StopClientVfx(vfxParams.InstanceId);
    }

    public static void StopClientVfx(Guid instanceId)
    {
        if (Instance == null || !Instance.Multiplayer.IsServer()) return;
        Instance.Rpc(MethodName.RpcStopClientVfx, instanceId.ToString());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RpcSpawnClientVfx(string cardIdStr, string instanceIdStr, int vfxIndex, Vector3 pos, Quaternion rot, Vector3 scale, bool attach)
    {
        if (!Guid.TryParse(cardIdStr, out var cardId) || !Guid.TryParse(instanceIdStr, out var instanceId)) return;
        if (!GameData.Instance.Cards.TryGet(cardId, out var def) || def.Visuals.Vfx == null) return;

        var vfxPrefab = def.Visuals.Vfx[vfxIndex];
        var instance = vfxPrefab.Instantiate<Node3D>();

        if (attach)
        {
            _spawnContainer.AddChild(instance);
        }
        else
        {
            _spawnContainer.AddChild(instance);
            instance.GlobalPosition = pos;
            instance.Quaternion = rot;
        }

        instance.Scale = scale;

        var controller = instance.GetComponentInChildren<IVfx>(required: false);
        controller?.Initialize(new VfxSpawnParams(pos) { Rotation = rot, Scale = scale });

        _activeClientVfx[instanceId] = instance;
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RpcStopClientVfx(string instanceIdStr)
    {
        if (!Guid.TryParse(instanceIdStr, out var instanceId)) return;
        if (!_activeClientVfx.Remove(instanceId, out var instance)) return;

        var vfx = instance.GetComponentInChildren<IVfx>(required: false);
        vfx?.Stop();
    }
}