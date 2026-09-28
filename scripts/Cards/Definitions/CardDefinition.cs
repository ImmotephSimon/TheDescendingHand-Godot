using System.Security.Cryptography;
using System.Text;

public abstract partial class CardDefinition : Resource
{
    private Guid? _cachedId;

    [Export] private float _castTime = 1f;
    [Export] private bool _spawnAtCursor = false;
    [Export] private CardVisuals _visuals = new();
    [Export] private DeckOverrides _deckOverrides = new();

    public Guid Id
    {
        get
        {
            if (!_cachedId.HasValue)
            {
                if (string.IsNullOrEmpty(ResourcePath))
                    return Guid.Empty;

                byte[] hash = MD5.HashData(Encoding.UTF8.GetBytes(ResourcePath));
                _cachedId = new Guid(hash);
            }

            return _cachedId.Value;
        }
    }

    public float CastTime => _castTime;
    public bool SpawnAtCursor => _spawnAtCursor;
    public CardVisuals Visuals => _visuals;
    public DeckOverrides DeckOverrides => _deckOverrides;

    public abstract void Construct(CardInitContext context, CardRuntime card);
}

public struct VfxSpawnParams
{
    public Guid InstanceId;
    public int VfxIndex;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public float Duration;
    public bool Attach;

    public VfxSpawnParams(Vector3 position, int vfxIndex = 0, float duration = 0f, bool attach = false)
    {
        InstanceId = Guid.NewGuid();
        VfxIndex = vfxIndex;
        Position = position;
        Duration = duration;
        Rotation = Quaternion.Identity;
        Scale = Vector3.One;
        Attach = attach;
    }

    public Transform3D ToTransform3D()
    {
        return new Transform3D(new Basis(Rotation), Position).ScaledLocal(Scale);
    }
}

public readonly struct CardInitContext
{
    public readonly Guid InstanceId;
    public readonly IEntity Owner;
    public readonly Func<Node, Node> ServerSpawn;
    public readonly Func<CardDefinition, VfxSpawnParams, Action> ClientSpawn;

    public CardInitContext(
        Guid instanceId,
        IEntity owner)
    {
        InstanceId = instanceId;
        Owner = owner;
    }
}

