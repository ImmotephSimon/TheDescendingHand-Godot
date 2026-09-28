[Tool]
[GlobalClass]
public partial class DoorSocket : Node3D
{
    public enum DoorType
    {
        Normal,
        Entrance
    }

    public enum DoorSide
    {
        North,
        South,
        East,
        West
    }

    [Export]
    private DoorSide side;
    public DoorSide Side
    {
        get => side;
        set
        {
            side = value;
            NotifyRoomUpdate();
        }
    }

    [Export(PropertyHint.Range, "-10,10,0.1")]
    private Vector2 offset;
    public Vector2 Offset
    {
        get => offset;
        set
        {
            offset = value;
            NotifyRoomUpdate();
        }
    }

    [Export]
    private DoorType type;
    public DoorType Type
    {
        get => type;
        set
        {
            type = value;
            NotifyRoomUpdate();
        }
    }

    private float width = 6f;
    public float Width => width;

    private DoorSocket connectedDoor;
    public DoorSocket ConnectedDoor => connectedDoor;
    public bool IsSocketConnected => connectedDoor != null;

    public void UpdatePosition(Aabb bounds, float roomUnit)
    {
        Vector3 position = bounds.GetCenter() + GetSideOffset(side, bounds.Size);

        position.Y += offset.Y * roomUnit;

        if (side == DoorSide.North || side == DoorSide.South)
            position.X += offset.X * roomUnit;
        else
            position.Z += offset.X * roomUnit;

        Position = position;

        Vector3 forward = GetForward(side);
        if (forward != Vector3.Zero)
        {
            Transform = new Transform3D(
                Basis.LookingAt(forward, Vector3.Up),
                position);
        }
    }

    private Vector3 GetSideOffset(DoorSide side, Vector3 size)
    {
        return side switch
        {
            DoorSide.North => new Vector3(0, 0, size.Z / 2),
            DoorSide.South => new Vector3(0, 0, -size.Z / 2),
            DoorSide.East => new Vector3(size.X / 2, 0, 0),
            DoorSide.West => new Vector3(-size.X / 2, 0, 0),
            _ => Vector3.Zero
        };
    }

    private Vector3 GetForward(DoorSide side)
    {
        return side switch
        {
            DoorSide.North => new Vector3(0, 0, 1),
            DoorSide.South => new Vector3(0, 0, -1),
            DoorSide.East => new Vector3(1, 0, 0),
            DoorSide.West => new Vector3(-1, 0, 0),
            _ => Vector3.Zero
        };
    }

    private void NotifyRoomUpdate()
    {
        if (Engine.IsEditorHint())
        {
            GetParent()?.Call("UpdateDoors");
        }
    }

    public float GetOffsetWorld(float roomUnit)
    {
        return offset.X * roomUnit;
    }

    public void Connect(DoorSocket other)
    {
        connectedDoor = other;
    }

    public void Disconnect()
    {
        connectedDoor = null;
    }

    public bool MatchesDirection(Vector2I direction)
    {
        return direction switch
        {
            var d when d == Vector2I.Up => side == DoorSide.North,
            var d when d == Vector2I.Down => side == DoorSide.South,
            var d when d == Vector2I.Right => side == DoorSide.East,
            var d when d == Vector2I.Left => side == DoorSide.West,
            _ => false
        };
    }
}