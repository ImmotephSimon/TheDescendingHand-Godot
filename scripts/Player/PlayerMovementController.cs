[GlobalClass]
public partial class PlayerMovementController : Node, IPlayerMovement
{
    private const float AccelerationMultiplier = 12f;
    private const float DecelerationMultiplier = 16f;
    private const float BaseDodgeDuration = 2.033f;
    private const float DodgeTurnSpeed = 180f;
    private const float AnimationReferenceSpeed = 4f;
    private const float SprintThreshold = 5f;

    [Export] private Curve _dodgeSpeedCurve = new();

    public enum MovementAction : byte
    {
        None,
        DodgeRoll
    }

    private PlayerAnimationController _animationHandler;
    private AnimationTree _animator;
    private IStatContainer _statContainer;

    private Vector3 _velocity;
    private Vector3 _currentMoveVelocity;
    private Vector2 _bufferedInput;
    private Vector3 _bufferedMousePos;

    private MovementAction _currentAction;
    private float _actionTimer;
    private MovementAction _bufferedAction;

    private float _networkedMoveSpeed = 2.5f;
    private float _networkedDodgeDuration = BaseDodgeDuration;

    public Vector3 CursorPosition => _bufferedMousePos;
    public float MoveSpeed => _networkedMoveSpeed;

    private CharacterBody3D _body;

    public override void _Ready()
    {
        ClientBridge.Instance.OnClientPlayerReady += Initialize;
    }

    private void Initialize(ClientPlayer clientPlayer)
    {
        _body = this.GetComponentInOwner<CharacterBody3D>();
        _animationHandler = clientPlayer.GetComponentInOwner<PlayerAnimationController>();
        _animator = _animationHandler.AnimationTree;


        _statContainer = this.GetComponentInOwner<IStatContainer>(required: false);
        _statContainer.Listen(GameTags.ModStatMovement, OnMovementStatChanged);
        OnMovementStatChanged(_statContainer.GetStat(GameTags.ModStatMovement));
    }

    public override void _ExitTree()
    {
        if (_statContainer != null)
        {
            _statContainer.StopListening(GameTags.ModStatMovement, OnMovementStatChanged);
        }
    }

    private void OnMovementStatChanged(float newValue)
    {
        _networkedMoveSpeed = newValue;
        var (_, additive, multiplicative) = _statContainer.GetScaling(GameTags.ModStatMovement);
        _networkedDodgeDuration = BaseDodgeDuration / ((1f + additive) * multiplicative);
    }

    public void SetLocalInput(Vector2 input, Vector3 mousePos)
    {
        _bufferedMousePos = mousePos;
        _bufferedInput = !IsImmobilized() ? input : Vector2.Zero;
    }

    private static bool IsImmobilized()
    {
        if (ClientBridge.Instance?.Stats == null) return false;
        return GameTags.Immobilizations.Any(tag => ClientBridge.Instance.Stats.Statuses.Contains(tag.TagId));
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (IsImmobilized())
        {
            _currentMoveVelocity = Vector3.Zero;
            ApplyGravity(dt);
            _body.MoveAndSlide();
            return;
        }

        ApplyGravity(dt);
        ProcessMovement(dt);
        _body.MoveAndSlide();
    }

    private void ApplyGravity(float dt)
    {
        if (_body.IsOnFloor() && _velocity.Y < 0f)
            _velocity.Y = -2f;
        else
            _velocity.Y += -9.81f * dt;
    }

    private void ProcessMovement(float dt)
    {
        Vector3 movementInput = new Vector3(_bufferedInput.X, 0f, _bufferedInput.Y).Normalized();

        if (_currentAction == MovementAction.None && _bufferedAction == MovementAction.DodgeRoll)
        {
            StartDodgeRoll();
            _bufferedAction = MovementAction.None;
        }

        if (_currentAction == MovementAction.DodgeRoll)
        {
            ProcessDodgeRollAction(movementInput, dt);
        }
        else
        {
            ProcessLocomotion(movementInput, _bufferedMousePos, dt);
        }

        _body.Velocity = _currentMoveVelocity + new Vector3(0f, _velocity.Y, 0f);
    }

    private void StartDodgeRoll()
    {
        _currentAction = MovementAction.DodgeRoll;
        _animationHandler?.PlayDodgeRoll(_networkedDodgeDuration);
        _actionTimer = _networkedDodgeDuration;
    }

    private void ProcessDodgeRollAction(Vector3 movementInput, float dt)
    {
        _actionTimer -= dt;
        float progress = 1f - (_actionTimer / _networkedDodgeDuration);
        float speedMultiplier = _dodgeSpeedCurve.Sample(progress);

        Vector3 moveDir = movementInput.LengthSquared() > 0.001f ? movementInput : _body.GlobalTransform.Basis.Z;
        _currentMoveVelocity = moveDir * (_networkedMoveSpeed * speedMultiplier);

        if (movementInput.LengthSquared() > 0.001f)
        {
            Quaternion targetRotation = Quaternion.FromEuler(new Vector3(0, Mathf.Atan2(moveDir.X, moveDir.Z), 0));
            _body.GlobalRotation = _body.GlobalRotation.Slerp(targetRotation.GetEuler(), DodgeTurnSpeed * Mathf.DegToRad(dt));
        }

        if (_actionTimer <= 0f)
        {
            _currentAction = MovementAction.None;
            _actionTimer = 0f;
            _currentMoveVelocity = Vector3.Zero;
        }
    }

    private void ProcessLocomotion(Vector3 movementInput, Vector3 mouseWorldPosition, float dt)
    {
        Vector3 direction = mouseWorldPosition - _body.GlobalPosition;
        direction.Y = 0f;

        if (direction.LengthSquared() > 0.001f)
            _body.LookAt(_body.GlobalPosition + direction, Vector3.Up, true);

        if (_networkedMoveSpeed <= 0f)
        {
            _currentMoveVelocity = Vector3.Zero;
            return;
        }

        Vector3 targetVelocity = movementInput * _networkedMoveSpeed;
        float rate = movementInput.LengthSquared() > 0.001f ? _networkedMoveSpeed * AccelerationMultiplier : _networkedMoveSpeed * DecelerationMultiplier;

        _currentMoveVelocity = new Vector3(
            Mathf.MoveToward(_currentMoveVelocity.X, targetVelocity.X, rate * dt),
            0f,
            Mathf.MoveToward(_currentMoveVelocity.Z, targetVelocity.Z, rate * dt)
        );
    }

    // Add this field to your class declarations:
    private Vector2 _currentBlendDir;

    public override void _Process(double delta)
    {
        if (_animator == null || _body == null) return;

        Vector3 movementInput = new Vector3(
            _bufferedInput.X,
            0f,
            _bufferedInput.Y
        ).Normalized();

        Vector3 localMovement =
            _body.GlobalTransform.Basis.Inverse() * movementInput;

        Vector2 targetPos = Vector2.Zero;

        if (localMovement.LengthSquared() > 0.001f)
        {
            Vector3 dirNorm = localMovement.Normalized();
            targetPos = new Vector2(dirNorm.X, -dirNorm.Z);
        }

        _currentBlendDir = _currentBlendDir.Lerp(
            targetPos,
            15f * (float)delta
        );

        _animator.Set(_animationHandler.RunBlendPosParam, _currentBlendDir);
        _animator.Set(_animationHandler.SprintBlendPosParam, _currentBlendDir);
        

        float currentVelocity = _currentMoveVelocity.Length();
        float sprintRatio = currentVelocity >= SprintThreshold ? 1f : 0f;

        _animator.Set(_animationHandler.RunSprintBlendParam, sprintRatio);

        //GD.Print(
        //    $"[AnimDebug] RawInput: {_bufferedInput} | " +
        //    $"LocalMove: {localMovement} | " +
        //    $"VelocityLen: {currentVelocity:F2} | " +
        //    $"BlendDir: {_currentBlendDir}"
        //);
    }


    public void DodgeRoll()
    {
        ClientBridge.Instance?.AbilitySystem?.RequestCancelCurrentCast();
        _bufferedAction = MovementAction.DodgeRoll;
    }
}