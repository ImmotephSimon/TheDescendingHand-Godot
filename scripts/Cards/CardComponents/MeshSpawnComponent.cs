[GlobalClass]
public partial class MeshSpawnComponent : CardComponent
{
    private PackedScene _prefab;
    private Vector3 _offset;
    private bool _useGravity;
    private float _scale = 1f;

    public event Action<Node3D> OnSpawned;

    public void Configure(
        PackedScene prefab,
        Vector3 offset = default,
        bool useGravity = false,
        float scale = 1f)
    {
        _prefab = prefab;
        _offset = offset;
        _useGravity = useGravity;
        _scale = scale;
    }

    protected override void OnActivate()
    {
        if (_prefab?.Instantiate() is not Node3D spawned)
            return;

        // Position & Scale
        spawned.GlobalPosition = Card.TargetLocation + _offset;
        spawned.GlobalRotation = CardOwner.Node.GlobalRotation;
        spawned.Scale = Vector3.One * _scale;

        // Godot physics configuration
        if (spawned is RigidBody3D rb)
        {
            rb.Freeze = !_useGravity; // Replaces isKinematic
            rb.GravityScale = _useGravity ? 1f : 0f;
        }

        // Network Spawn: Simply add to the active multiplayer spawner container on server
        if (Multiplayer.IsServer())
        {
            NetworkManager.Spawn(spawned); // Calls _spawnerNode.AddChild(spawned, forceReadableName: true)
        }

        OnSpawned?.Invoke(spawned);
    }
}