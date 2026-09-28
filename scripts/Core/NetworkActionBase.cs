public abstract partial class NetworkActionBase : Node
{
    [Export] private PackedScene _cardPrefab;
    private IEntity _owner;
    private readonly Dictionary<Guid, Node> _activeClientVfx = new();

    private const int MaxSpawnedObjects = 50;
    private static readonly Queue<Node> SpawnedObjects = new();

    public override void _Ready()
    {
        _owner = this.GetComponentInOwner<IEntity>();

        if (Multiplayer.IsServer())
        {
            CardFactory.Initialize(
                registry: GameData.Instance.Cards,
                cardRuntimePrefab: _cardPrefab
            );
        }
    }

    public CardInstance CreateCard(CardDefinition definition)
    {
        if (!Multiplayer.IsServer())
            throw new InvalidOperationException("[NetworkActionBase] CreateCard can only be called on the server.");

        return CardFactory.CreateCardInstance(definition, _owner);
    }

    protected Action SpawnClientVfx(CardDefinition cardDefinition, VfxSpawnParams vfxParams)
    {
        Vector3 direction = (vfxParams.Position - _owner.Node.GlobalPosition).Normalized();
        direction.Y = 0f;
        vfxParams.Rotation = new Quaternion(Vector3.Forward, direction);

        Rpc(MethodName.SpawnClientVfxRpc,
            cardDefinition.Id.ToString(),
            vfxParams.InstanceId.ToString(),
            vfxParams.VfxIndex,
            vfxParams.Position,
            vfxParams.Rotation,
            vfxParams.Scale,
            vfxParams.Attach);

        return () => StopClientVfx(vfxParams.InstanceId);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void SpawnClientVfxRpc(string cardIdStr, string instanceIdStr, int vfxIndex, Vector3 pos, Quaternion rot, Vector3 scale, bool attach)
    {
        if (!Guid.TryParse(cardIdStr, out var cardId) || !Guid.TryParse(instanceIdStr, out var instanceId)) return;
        if (!GameData.Instance.Cards.TryGet(cardId, out var def) || def.Visuals.Vfx == null) return;

        var vfxPrefab = def.Visuals.Vfx[vfxIndex];
        var instance = vfxPrefab.Instantiate<Node3D>();

        if (attach)
        {
            AddChild(instance);
        }
        else
        {
            GetTree().CurrentScene.AddChild(instance);
            instance.GlobalPosition = pos;
            instance.Quaternion = rot;
        }

        instance.Scale = scale;

        var controller = instance.GetComponentInChildren<IVfx>();
        controller?.Initialize(new VfxSpawnParams(pos) { Rotation = rot, Scale = scale });

        _activeClientVfx[instanceId] = instance;
    }

    public void StopClientVfx(Guid instanceId)
    {
        if (!Multiplayer.IsServer()) return;
        Rpc(MethodName.StopClientVfxRpc, instanceId.ToString());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void StopClientVfxRpc(string instanceIdStr)
    {
        if (!Guid.TryParse(instanceIdStr, out var instanceId)) return;
        if (!_activeClientVfx.Remove(instanceId, out var instance)) return;

        var vfx = instance.GetComponentInChildren<IVfx>();
        vfx?.Stop();
    }


}