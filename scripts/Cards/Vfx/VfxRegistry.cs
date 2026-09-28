[GlobalClass]
public partial class VfxRegistry : Resource
{
    [Export] private Godot.Collections.Array<VfxEntry> entries = new();

    private Dictionary<PackedScene, string> _prefabToId;
    private Dictionary<string, PackedScene> _idToPrefab;


    public void Initialize()
    {
        if (_prefabToId != null)
            return;

        _prefabToId = new Dictionary<PackedScene, string>();
        _idToPrefab = new Dictionary<string, PackedScene>();

        foreach (var entry in entries)
        {
            if (entry == null || entry.Prefab == null || string.IsNullOrEmpty(entry.Id))
                continue;

            _prefabToId[entry.Prefab] = entry.Id;
            _idToPrefab[entry.Id] = entry.Prefab;
        }
    }

    public bool TryGetId(PackedScene prefab, out string id)
    {

        if (prefab != null && _prefabToId.TryGetValue(prefab, out id))
            return true;

        GD.PrintErr($"[VfxRegistry] No ID registered for prefab '{prefab?.ResourcePath}'.");
        id = null;
        return false;
    }

    public bool TryGetPrefab(string id, out PackedScene prefab)
    {

        if (_idToPrefab.TryGetValue(id, out prefab))
            return true;

        GD.PrintErr($"[VfxRegistry] No prefab registered for ID '{id}'.");
        prefab = null;
        return false;
    }

    public void ValidateEntries()
    {
        HashSet<string> seenIds = new();
        HashSet<PackedScene> seenPrefabs = new();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry == null) continue;

            if (string.IsNullOrEmpty(entry.Id) || !seenIds.Add(entry.Id))
            {
                entry.SetId(Guid.NewGuid().ToString());
                seenIds.Add(entry.Id);
            }

            if (entry.Prefab != null && !seenPrefabs.Add(entry.Prefab))
            {
                GD.PrintErr($"[VfxRegistry] Duplicate Prefab assigned: {entry.Prefab.ResourcePath}");
            }
        }

        _prefabToId = null;
        _idToPrefab = null;
    }
}

