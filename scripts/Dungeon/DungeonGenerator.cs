[GlobalClass]
public partial class DungeonGenerator : Node3D
{
    [Export] private DungeonConfig config;
    [Export] private int roomUnit = 3;
    [Export] private NavigationRegion3D navMeshRegion;
    public static DungeonGenerator Instance { get; private set; }

    private const int MaxGenerationAttempts = 10;
    private const int BossDepth = 5;
    private List<Room> generatedRooms = new();

    private Dictionary<Vector2I, Room> dungeonMap = new();
    private DungeonDecorator decorator;
    public DungeonGenerator ParentDungeon { get; set; }
    public int ZoneLevel = 0;

    public override void _EnterTree() => Instance = this;

    public void StartGenerating()
    {
        if (decorator == null) decorator = this.GetComponentInChildren<DungeonDecorator>();

        for (int attempt = 1; attempt <= MaxGenerationAttempts; attempt++)
        {
            ClearDungeon();

            if (TryGenerateDungeon())
            {
                GD.Print($"[DungeonGenerator] Dungeon generated successfully on attempt {attempt}");
                FinishDungeon();
                return;
            }
            GD.Print($"[DungeonGenerator] Dungeon generation failed, retrying ({attempt}/{MaxGenerationAttempts})");
        }

        GD.PrintErr($"[DungeonGenerator] Dungeon generation failed after all ({MaxGenerationAttempts}) attempts");
    }

    bool TryGenerateDungeon()
    {
        var startPrefab = config.startRooms[GD.RandRange(0, config.startRooms.Length - 1)];
        Room startRoom = startPrefab.Instantiate<Room>();
        AddChild(startRoom);
        startRoom.GlobalTransform = GlobalTransform;

        AddStartRoom(startRoom);

        bool hasBranch = false;

        foreach (DoorSocket door in startRoom.GetUnconnectedDoors())
        {
            List<Room> branchRooms = new();

            if (GenerateBranch(door, 1, branchRooms))
            {
                hasBranch = true;
            }
            else
            {
                CreateDeadEnd(door);
            }
        }

        return hasBranch;
    }

    private void AddStartRoom(Room room)
    {
        generatedRooms.Add(room);
        RegisterRoom(room, room.GetRoomGridPosition());
    }

    void ClearDungeon()
    {
        foreach (Room room in generatedRooms)
        {
            if (GodotObject.IsInstanceValid(room))
                room.Free();
        }

        generatedRooms.Clear();
        dungeonMap.Clear();
    }

    void Rollback(List<Room> rooms)
    {
        foreach (Room room in rooms)
        {
            if (GodotObject.IsInstanceValid(room))
            {
                foreach (var pair in dungeonMap.Where(p => p.Value == room).ToList())
                {
                    dungeonMap.Remove(pair.Key);
                }
                generatedRooms.Remove(room);
                room.GetEntranceDoor().Disconnect();
                room.Free();
            }
        }
        rooms.Clear();
    }

    bool GenerateBranch(DoorSocket parentDoor, int depth, List<Room> createdRooms)
    {
        if (depth >= BossDepth)
        {
            if (TrySpawnRoom(parentDoor, config.bossRooms, out Room boss))
            {
                createdRooms.Add(boss);
                return true;
            }
            return false;
        }

        if (!TrySpawnRoom(parentDoor, config.standardRooms, out Room room))
        {
            return false;
        }

        createdRooms.Add(room);
        bool hasSuccessfulChild = false;

        foreach (DoorSocket door in room.GetUnconnectedDoors())
        {
            List<Room> childRooms = new();

            if (GenerateBranch(door, depth + 1, childRooms))
            {
                createdRooms.AddRange(childRooms);
                hasSuccessfulChild = true;
            }
            else
            {
                CreateDeadEnd(door);
            }
        }

        if (!hasSuccessfulChild)
        {
            Rollback(createdRooms);
            return false;
        }

        return true;
    }

    bool TrySpawnRoom(DoorSocket parentDoor, PackedScene[] options, out Room room)
    {
        var shuffled = options.OrderBy(x => GD.Randf()).ToArray();

        foreach (PackedScene prefab in shuffled)
        {
            if (prefab == null) continue;

            Room spawnedRoom = prefab.Instantiate<Room>();
            AddChild(spawnedRoom);
            spawnedRoom.GlobalTransform = Transform3D.Identity;

            Vector2I gridPosition = SnapRoom(parentDoor, spawnedRoom, spawnedRoom.GetEntranceDoor());

            if (CanPlaceRoom(spawnedRoom, gridPosition))
            {
                AcceptRoom(parentDoor, spawnedRoom, gridPosition);
                room = spawnedRoom;
                return true;
            }

            spawnedRoom.Free();
        }

        room = null;
        GD.Print($"[DungeonGenerator] No valid room found from door {parentDoor.Name}");
        return false;
    }

    private void AcceptRoom(DoorSocket parentDoor, Room room, Vector2I gridPosition)
    {
        GD.Print($"[DungeonGenerator] Accepted room {room.Name} at {room.GlobalPosition}");

        generatedRooms.Add(room);
        RegisterRoom(room, gridPosition);

        parentDoor.Connect(room.GetEntranceDoor());
        room.GetEntranceDoor().Connect(parentDoor);
    }

    void CreateDeadEnd(DoorSocket parentDoor)
    {
        Room room = config.deadEndRoom.Instantiate<Room>();
        AddChild(room);
        room.GlobalTransform = Transform3D.Identity;

        Vector2I gridPosition = SnapRoom(parentDoor, room, room.GetEntranceDoor());

        if (CanPlaceRoom(room, gridPosition))
        {
            AcceptRoom(parentDoor, room, gridPosition);
            GD.Print($"[DungeonGenerator] Spawned DEAD END at {room.GlobalPosition}");
        }
        else
        {
            room.Free();
            parentDoor.Disconnect();
            GD.Print("[DungeonGenerator] Dead end rejected due to overlap");
        }
    }

    bool CanPlaceRoom(Room room, Vector2I origin)
    {
        foreach (Vector2I cell in GetOccupiedCells(room))
        {
            Vector2I position = origin + cell;

            if (dungeonMap.TryGetValue(position, out Room existing))
            {
                GD.Print($"Overlap: {room.Name} wants {position}, occupied by {existing.Name}");
                return false;
            }
        }

        return true;
    }

    void RegisterRoom(Room room, Vector2I origin)
    {
        foreach (Vector2I cell in GetOccupiedCells(room))
        {
            Vector2I position = origin + cell;
            dungeonMap[position] = room;
        }
    }

    List<Vector2I> GetOccupiedCells(Room room)
    {
        Vector3 worldExtent = room.GlobalTransform.Basis * room.RoomExtent;

        Vector2I size = new(
            Mathf.RoundToInt(Mathf.Abs(worldExtent.X) / roomUnit),
            Mathf.RoundToInt(Mathf.Abs(worldExtent.Z) / roomUnit)
        );

        List<Vector2I> cells = new();
        int offsetX = size.X / 2;
        int offsetZ = size.Y / 2;

        for (int x = 0; x < size.X; x++)
        {
            for (int z = 0; z < size.Y; z++)
            {
                cells.Add(new Vector2I(x - offsetX, z - offsetZ));
            }
        }

        return cells;
    }

    Vector2I SnapRoom(DoorSocket parentDoor, Room room, DoorSocket childDoor)
    {
        Node3D childRoom = room;

        Basis lookBasis = Basis.LookingAt(-parentDoor.GlobalTransform.Basis.Z, Vector3.Up);
        Quaternion targetRot = lookBasis.GetRotationQuaternion() * childDoor.GlobalTransform.Basis.GetRotationQuaternion().Inverse();
        childRoom.GlobalTransform = new Transform3D(new Basis(targetRot), childRoom.GlobalPosition);

        Vector3 offset = childDoor.GlobalPosition - parentDoor.GlobalPosition;
        childRoom.GlobalPosition -= offset;

        return room.GetRoomGridPosition();
    }

    void FinishDungeon()
    {
        decorator.Decorate(dungeonMap, roomUnit);
        navMeshRegion.BakeNavigationMesh();
    }
}