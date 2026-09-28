using System.Threading.Tasks;

[GlobalClass]
public partial class ServerPlayer : Entity, IPlayerCollection
{
    [Export] private StatModifierData _startingStats;
    [Export(PropertyHint.Layers3DPhysics)] private uint _interactLayer = 1;
    [Export] private float _interactRange = 2f;
    [Export] private TriggerDetector _enemyDetector;

    private PlayerMovementController _playerMovement;
    private CardController _cardController;
    private ModifierHandle _sprintHandle = ModifierHandle.Invalid;
    private ModifierHandle _movementLockHandle;

    public CardController CardController => _cardController;
    public float InteractRange => _interactRange;
    public int PeerId => GetMultiplayerAuthority();

    protected override Node ComponentRoot => this.GetParent().GetComponentInOwner<PlayerRoot>();

    public override Vector3 GetCursorPosition() => _playerMovement.CursorPosition;

    public override void _EnterTree()
    {
        base._EnterTree();
        Owner = ComponentRoot;
        _playerMovement = this.GetComponentInOwner<PlayerMovementController>();
        _animationHandler = this.GetComponentInOwner<IAnimationHandler>();
        _cardController = this.GetComponentInOwner<CardController>();

        if (_enemyDetector != null)
        {
            _enemyDetector.OnEntered += _ => ToggleSprint(false);
            _enemyDetector.OnExited += _ => ToggleSprint(_enemyDetector.Count == 0);
        }

        ToggleSprint(true);
        _startingStats?.ApplyTo(_stats);
    }

    public void SetPeerId(int peerId)
    {
        SetMultiplayerAuthority(peerId);
    }

    private void ToggleSprint(bool isEnabled)
    {
        if (isEnabled)
        {
            if (_sprintHandle.IsValid) return;

            _sprintHandle = _stats.AddModifier(
                new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 1.8f));
        }
        else if (_sprintHandle.IsValid)
        {
            _stats.RemoveModifier(_sprintHandle);
            this.GetComponentInOwner<PlayerNetworkActions>()?.ShowSpeechBubble("What was that?");
        }
    }

    protected override void OnDeath(IEntity killer)
    {
        if (!Multiplayer.IsServer()) return;

        _movementLockHandle = Stats.AddModifier(new StatModifier(GameTags.ModStatMovement, MathOp.Multiplicative, 0));
        _ = ScheduleRevivalAsync();
    }

    private async Task ScheduleRevivalAsync()
    {
        await ToSignal(GetTree().CreateTimer(4.0f), SceneTreeTimer.SignalName.Timeout);

        if (!GodotObject.IsInstanceValid(this)) return;

        var healthHandler = this.GetComponentInOwner<IHealth>();
        healthHandler?.AdjustHealth(healthHandler.MaxHealth, this);

        IsDead = false;
        Stats.RemoveModifier(_movementLockHandle);
        _animationHandler?.SetAnimationState(CharacterAnimationState.Locomotion);

        GameWorld.Instance?.NotifyRevive(this);
    }

    protected override void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (victim != null && victim.HostileLayer == TeamLayer)
        {
            GD.Print($"Provide exp from {victim} death");
        }
    }

    public IInteractable FindNearbyInteractable()
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = new SphereShape3D { Radius = _interactRange },
            Transform = GlobalTransform,
            CollisionMask = _interactLayer
        };

        var results = spaceState.IntersectShape(query);
        foreach (var result in results)
        {
            var collider = result["collider"].As<Node>();
            if (collider != null && collider.TryGetComponent<IInteractable>(out var interactable))
                return interactable;
        }

        return null;
    }

    public override void _ExitTree()
    {
        _startingStats?.RemoveFrom(_stats);
        base._ExitTree();
    }

    public void AddGold(int amount) { }
}