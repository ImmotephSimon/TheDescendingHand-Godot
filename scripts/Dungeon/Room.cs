[GlobalClass]
public partial class Room : Node3D
{
    [Export] public float roomUnit = 3f;
    [Export] public CollisionShape3D bounds;

    private DoorSocket[] doorSockets;
    private DoorSocket entranceDoor;
    private readonly List<HingeController> _doors = new();

    public List<HingeController> Doors => _doors;

    public Vector3 RoomExtent => bounds?.Shape is BoxShape3D box
        ? box.Size
        : throw new InvalidOperationException($"Room '{Name}' is missing a valid BoxShape3D bounds shape.");

    public CollisionShape3D Bounds => bounds;

    public event Action Entered;
    private bool entered = false;

    public override void _Ready()
    {
        doorSockets = this.GetComponentsInChildren<DoorSocket>().ToArray();
        CheckGridAlignment();
    }

    public void UpdateDoors()
    {
        doorSockets = this.GetComponentsInChildren<DoorSocket>().ToArray();
        foreach (var socket in doorSockets)
            socket.UpdatePosition(new Aabb(-RoomExtent / 2f, RoomExtent), roomUnit);
    }

    private void CheckGridAlignment()
    {
        if (bounds?.Shape is not BoxShape3D box) return;

        float x = box.Size.X / roomUnit;
        float z = box.Size.Z / roomUnit;

        bool invalidX = !Mathf.IsEqualApprox(x, Mathf.Round(x));
        bool invalidZ = !Mathf.IsEqualApprox(z, Mathf.Round(z));

        if (invalidX || invalidZ)
        {
            GD.PrintErr(
                $"Room '{Name}' has invalid extent {box.Size}. " +
                $"roomUnit={roomUnit}. " +
                $"X={x:F2} tiles, Z={z:F2} tiles. " +
                $"CollisionShape='{bounds.Name}'.");
        }
    }

    public DoorSocket GetEntranceDoor()
    {
        if (entranceDoor == null)
        {
            List<DoorSocket> availableDoors = GetUnconnectedDoors();

            entranceDoor = availableDoors.Count > 0
                ? availableDoors[(int)(GD.Randi() % (uint)availableDoors.Count)]
                : availableDoors.FirstOrDefault();
        }
        return entranceDoor;
    }

    public List<DoorSocket> GetUnconnectedDoors()
    {
        return doorSockets.Where(d => !d.IsSocketConnected).ToList();
    }

    public List<DoorSocket> GetConnectedDoors()
    {
        return doorSockets.Where(d => d.IsSocketConnected).ToList();
    }

    private void OnDoorEntered()
    {
        if (entered)
            return;

        entered = true;
        foreach (HingeController door in _doors)
        {
            door.PlayerInteracted -= OnDoorEntered;
        }
        Entered?.Invoke();
    }

    public void RegisterDoor(HingeController door)
    {
        if (door == null)
            return;

        _doors.Add(door);
        door.PlayerInteracted += OnDoorEntered;
    }

    public Vector2I GetRoomGridPosition()
    {
        return new Vector2I(
            Mathf.RoundToInt(GlobalPosition.X / roomUnit),
            Mathf.RoundToInt(GlobalPosition.Z / roomUnit)
        );
    }
}