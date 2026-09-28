[GlobalClass]
public partial class DungeonConfig : Resource
{
    [Export] public PackedScene[] startRooms;
    [Export] public PackedScene[] standardRooms;
    [Export] public PackedScene[] bossRooms;
    [Export] public PackedScene deadEndRoom;
}