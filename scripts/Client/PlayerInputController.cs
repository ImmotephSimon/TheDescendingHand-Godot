
[GlobalClass]
public partial class PlayerInputController : Node
{
    [Export(PropertyHint.Layers3DPhysics)]
    private uint _floorCollisionMask = 1;
    private CharacterBody3D _player;
    private IPlayerMovement _movement;
    private Vector2 _rawInput;

    public override void _Ready()
    {
        ClientBridge.Instance.OnClientPlayerReady += Initialize;
    }

    private void Initialize(ClientPlayer player)
    {
        _player = ClientBridge.Instance.Root.GetComponentInOwner<CharacterBody3D>();
        _movement = ClientBridge.Instance.Root.GetComponentInOwner<IPlayerMovement>();
    }

    public override void _Process(double delta)
    {
        UpdateMovementInput();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("card_1")) OnPlayCard(0);
        else if (@event.IsActionReleased("card_1")) OnReleaseCard(0);
        else if (@event.IsActionPressed("card_2")) OnPlayCard(1);
        else if (@event.IsActionReleased("card_2")) OnReleaseCard(1);
        else if (@event.IsActionPressed("card_3")) OnPlayCard(2);
        else if (@event.IsActionReleased("card_3")) OnReleaseCard(2);
        else if (@event.IsActionPressed("card_4")) OnPlayCard(3);
        else if (@event.IsActionReleased("card_4")) OnReleaseCard(3);
        else if (@event.IsActionPressed("card_5")) OnPlayCard(4);
        else if (@event.IsActionReleased("card_5")) OnReleaseCard(4);
        else if (@event.IsActionPressed("dodge")) _movement.DodgeRoll();
        else if (@event.IsActionPressed("loot")) OnLoot();
        else if (@event.IsActionPressed("inventory")) OnOpenInventory();
        else if (@event.IsActionPressed("passive_tree")) OnTogglePassiveTree();
        else if (@event is InputEventMouseButton mouseEvent &&
                 mouseEvent.Pressed &&
                 mouseEvent.ButtonIndex == MouseButton.Left)
        {
            OnLeftClick();
        }
    }

    private void OnPlayCard(int index)
    {
        GD.Print($"Playing index {index}");
        ClientBridge.Instance.AbilitySystem?.RequestUseAbility(index);
    }

    private void OnReleaseCard(int index)
    {
        ClientBridge.Instance.AbilitySystem?.RequestCancelAbility(index);
    }

    private void UpdateMovementInput()
    {
        if (_movement == null) return;

        float x = (Input.IsKeyPressed(Key.A) ? 1f : 0f) - (Input.IsKeyPressed(Key.D) ? 1f : 0f);
        float y = (Input.IsKeyPressed(Key.W) ? 1f : 0f) - (Input.IsKeyPressed(Key.S) ? 1f : 0f);
        _rawInput = new Vector2(x, y);



        Camera3D camera = GetViewport().GetCamera3D();
        if (camera == null) return;

        Vector2 mousePos = GetViewport().GetMousePosition();
        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
        Vector3 rayNormal = camera.ProjectRayNormal(mousePos);
        Vector3 rayEnd = rayOrigin + rayNormal * 1000f;

        var spaceState = camera.GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd, _floorCollisionMask);
        var result = spaceState.IntersectRay(query);

        Vector3 mouseWorldPosition = Vector3.Zero;

        if (result.Count > 0)
        {
            mouseWorldPosition = (Vector3)result["position"];
        }
        else
        {
            Plane plane = new Plane(Vector3.Up, _player.GlobalPosition.Y);
            Vector3? hitPos = plane.IntersectsRay(rayOrigin, rayNormal);
            if (hitPos.HasValue)
            {
                mouseWorldPosition = hitPos.Value;
            }
        }



            _movement.SetLocalInput(_rawInput, mouseWorldPosition);
        
    }

    private void OnLeftClick() => CursorItemController.Instance?.TryDropHeldItem();
    private void OnLoot() => ClientBridge.Instance?.ClientPlayer?.TryInteract();
    private void OnOpenInventory() => ClientBridge.Instance.PlayerHUD?.ToggleInventory();
    private void OnTogglePassiveTree() => ClientBridge.Instance.PlayerHUD?.TogglePassiveTree();
}