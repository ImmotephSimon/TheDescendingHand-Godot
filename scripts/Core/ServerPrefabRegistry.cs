[GlobalClass]
public partial class ServerPrefabRegistry : Resource
{

    [Export] private Godot.Collections.Array<ServerPrefabEntry> _entries = new();
    private Dictionary<GameTag, PackedScene> _lookup;

    public bool TryGetPrefab(GameTag tag, out PackedScene prefab)
    {
        if (_lookup.TryGetValue(tag, out prefab))
            return true;

        GD.PrintErr($"ServerPrefabRegistry: No prefab registered for tag '{tag}'.");
        return false;
    }

    public void Initialize()
    {
        _lookup = new Dictionary<GameTag, PackedScene>(_entries.Count);

        foreach (var entry in _entries)
        {
            if (entry != null && entry.Prefab != null)
                _lookup[entry.Tag] = entry.Prefab;
        }
    }
}