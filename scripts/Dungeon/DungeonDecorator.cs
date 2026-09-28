[GlobalClass]
public partial class DungeonDecorator : Node3D
{
    private const float FloorHeight = 0.5f;

    [Export] private PackedScene wallPrefab;
    [Export] private PackedScene doorWallPrefab;
    private int roomUnit;

    private Dictionary<Vector2I, Room> dungeonMap;

    public void Decorate(Dictionary<Vector2I, Room> dungeonMap, int roomUnit)
    {
        this.roomUnit = roomUnit;
        this.dungeonMap = dungeonMap;

        HashSet<string> processedConnections = new();

        foreach (Room room in dungeonMap.Values.Distinct())
        {
            foreach (DoorSocket door in room.GetConnectedDoors())
            {
                if (door == null || door.ConnectedDoor == null) continue;

                string idA = door.GetInstanceId().ToString();
                string idB = door.ConnectedDoor.GetInstanceId().ToString();
                string connectionKey = string.Compare(idA, idB, StringComparison.Ordinal) < 0 ? $"{idA}_{idB}" : $"{idB}_{idA}";

                if (processedConnections.Add(connectionKey))
                {
                    CreateDoorFrame(
                        room,
                        door.ConnectedDoor.GetComponentInParent<Room>(),
                        door
                    );
                }
            }
        }

        foreach (Vector2I cell in dungeonMap.Keys)
        {
            CheckWall(cell, Vector2I.Up);
            CheckWall(cell, Vector2I.Down);
            CheckWall(cell, Vector2I.Left);
            CheckWall(cell, Vector2I.Right);
        }
    }

    private void CreateDoorFrame(Room roomA, Room roomB, DoorSocket door)
    {
        Node3D obj = doorWallPrefab.Instantiate<Node3D>();
        roomA.AddChild(obj);
        obj.GlobalPosition = door.GlobalPosition;
        obj.GlobalRotation = door.GlobalRotation;

        HingeController controller = obj.GetComponentInChildren<HingeController>();

        roomA.RegisterDoor(controller);
        roomB?.RegisterDoor(controller);
    }

    private void CheckWall(Vector2I cell, Vector2I direction)
    {
        Room room = dungeonMap[cell];

        bool isBoundary =
            !dungeonMap.TryGetValue(cell + direction, out Room neighbour) ||
            neighbour != room;

        if (!isBoundary)
            return;

        float roomBottom = room.GlobalPosition.Y - room.RoomExtent.Y / 2f + roomUnit / 2f;
        float roomTop = room.GlobalPosition.Y + room.RoomExtent.Y / 2f;

        for (float y = roomBottom; y < roomTop; y += roomUnit)
        {
            if (HasDoorOnEdge(cell, direction, room, y))
                continue;

            CreateWallForEdge(cell, direction, room, y);
        }
    }

    private bool HasDoorOnEdge(Vector2I cell, Vector2I direction, Room room, float y)
    {
        if (!dungeonMap.TryGetValue(cell + direction, out Room neighbour))
            return false;

        foreach (DoorSocket door in room.GetConnectedDoors())
        {
            if (door.ConnectedDoor == null)
                continue;

            if (!ReferenceEquals(door.ConnectedDoor.GetComponentInParent<Room>(), neighbour))
            {
                continue;
            }

            Vector3 wallPosition = GridToWorldPosition(cell, direction, y);

            float distance = direction.X != 0
                ? Mathf.Abs(door.GlobalPosition.Z - wallPosition.Z)
                : Mathf.Abs(door.GlobalPosition.X - wallPosition.X);

            if (distance <= door.Width / 2f)
                return true;
        }

        return false;
    }

    private void CreateWallForEdge(Vector2I cell, Vector2I direction, Room room, float y)
    {
        Vector3 pos = GridToWorldPosition(cell, direction, y);

        Quaternion rotation = direction.X != 0
            ? Quaternion.Identity
            : Quaternion.FromEuler(new Vector3(0, Mathf.DegToRad(90), 0));

        Node3D wall = wallPrefab.Instantiate<Node3D>();
        room.AddChild(wall);
        wall.GlobalTransform = new Transform3D(new Basis(rotation), pos);
    }

    private Vector3 GridToWorldPosition(Vector2I cell, Vector2I direction, float y)
    {
        Room room = dungeonMap[cell];
        Vector2I roomGridPosition = room.GetRoomGridPosition();

        Vector3 pos = room.GlobalPosition;
        pos.Y = y + FloorHeight;

        float pivotOffset = roomUnit / 2f;

        pos += new Vector3(
            (cell.X - roomGridPosition.X) * roomUnit + pivotOffset,
            0,
            (cell.Y - roomGridPosition.Y) * roomUnit + pivotOffset
        );

        if (direction == Vector2I.Right)
            pos.X += pivotOffset;
        else if (direction == Vector2I.Left)
            pos.X -= pivotOffset;
        else if (direction == Vector2I.Up)
            pos.Z += pivotOffset;
        else if (direction == Vector2I.Down)
            pos.Z -= pivotOffset;

        return pos;
    }
}