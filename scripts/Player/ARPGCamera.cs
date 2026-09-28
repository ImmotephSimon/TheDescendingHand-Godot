[Tool]
public partial class ARPGCamera : Camera3D
{
    [Export] private float _cameraDistance = 8f;
    [Export] private float _cameraAngle = 55f;

    private Vector3 Offset => new Quaternion(Vector3.Right, Mathf.DegToRad(_cameraAngle)) * new Vector3(0f, 0f, -_cameraDistance);
    private Node3D _player;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _Process(double delta)
    {
        if (_player != null)
            SnapToPlayer();
    }

    public void SetPlayer(Node3D player)
    {
        _player = player;
    }

    private void SnapToPlayer()
    {
        Vector3 targetPos = _player.GlobalPosition;
        GlobalPosition = targetPos + Offset;
        LookAt(targetPos, Vector3.Up);

        //RenderingServer.GlobalShaderParameterSet("_PlayerPosition", targetPos + Vector3.Up);

        //Vector2 screenPos = UnprojectPosition(targetPos);
        //Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
        //RenderingServer.GlobalShaderParameterSet("_PlayerScreenPosition", new Vector2(screenPos.X / viewportSize.X, screenPos.Y / viewportSize.Y));
        //RenderingServer.GlobalShaderParameterSet("_CameraPosition", GlobalPosition);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton
            {
                ButtonIndex: MouseButton.Left,
                Pressed: true
            })
        {
            Vector2 mousePos = GetViewport().GetMousePosition();
            Vector3 rayOrigin = ProjectRayOrigin(mousePos);
            Vector3 rayEnd = rayOrigin + ProjectRayNormal(mousePos) * 1000f;

            var spaceState = GetWorld3D().DirectSpaceState;
            var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                Node colliderNode = result["collider"].As<Node>();
                GD.Print($"Click {colliderNode.Name}");
            }
        }
    }
}