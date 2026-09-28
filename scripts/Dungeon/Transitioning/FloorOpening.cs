using System.Threading.Tasks;

[GlobalClass]
public partial class FloorOpening : Node3D
{
    [Export] private Node3D[] _rightTiles;
    [Export] private Node3D[] _leftTiles;
    [Export] private float _dropDepth = 0.3f;
    [Export] private float _slideDistance = 3f;
    private float _moveSpeed = 1f;

    public async void OpenFloor()
    {
        await AnimateOpening();
    }

    private async Task AnimateOpening()
    {
        Vector3[] leftStarts = GetStartPositions(_leftTiles);
        Vector3[] rightStarts = GetStartPositions(_rightTiles);

        // Phase 1: Move both sides down
        Task dropLeft = MoveGroup(_leftTiles, leftStarts, Vector3.Down * _dropDepth);
        Task dropRight = MoveGroup(_rightTiles, rightStarts, Vector3.Down * _dropDepth);
        await Task.WhenAll(dropLeft, dropRight);

        // Phase 2: Slide apart using local space vectors
        Vector3 leftTargetOffset = (Vector3.Down * _dropDepth) + (Vector3.Left * _slideDistance);
        Vector3 rightTargetOffset = (Vector3.Down * _dropDepth) + (Vector3.Right * _slideDistance);

        Task moveLeft = MoveGroup(_leftTiles, leftStarts, leftTargetOffset);
        Task moveRight = MoveGroup(_rightTiles, rightStarts, rightTargetOffset);
        await Task.WhenAll(moveLeft, moveRight);
    }

    private static Vector3[] GetStartPositions(Node3D[] tiles)
    {
        Vector3[] starts = new Vector3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
            starts[i] = tiles[i].Position;
        return starts;
    }

    private async Task MoveGroup(Node3D[] tiles, Vector3[] startPositions, Vector3 offset)
    {
        bool moving = true;
        while (moving)
        {
            moving = false;
            float dt = (float)GetProcessDeltaTime();

            for (int i = 0; i < tiles.Length; i++)
            {
                Vector3 target = startPositions[i] + offset;
                tiles[i].Position = tiles[i].Position.MoveToward(target, _moveSpeed * dt);

                if (tiles[i].Position.DistanceTo(target) > 0.001f)
                    moving = true;
            }

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
}