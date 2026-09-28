[GlobalClass]
public partial class ServerPrefabEntry : Resource
{
    [Export] public GameTag Tag { get; set; }
    [Export] public PackedScene Prefab { get; set; }
}