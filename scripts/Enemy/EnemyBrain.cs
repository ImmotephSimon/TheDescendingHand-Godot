public enum BrainState
{
    Active,
    Suspended,
    Dead
}

[GlobalClass]
public partial class EnemyBrain : Node3D, ISpawnable
{
    private readonly List<IEnemyAction> _actions = new();
    private IEnemyAction _currentAction;
    private BrainState _state = BrainState.Active;

    public void AddAction(IEnemyAction action)
    {
        _actions.Add(action);
    }

    // Changed from Unity's Update() to Godot's _Process (or _PhysicsProcess depending on movement AI)
    public override void _Process(double delta)
    {
        if (_state != BrainState.Active) return;

        // Pass delta along to action updates
        float deltaFloat = (float)delta;

        if (_currentAction != null && !_currentAction.CanBeInterrupted)
        {
            _currentAction.UpdateAction(deltaFloat);
            return;
        }

        IEnemyAction bestAction = null;
        float bestScore = float.MinValue;

        foreach (var action in _actions)
        {
            if (!action.IsAvailable())
                continue;

            float score = action.GetPriority();

            if (score > bestScore)
            {
                bestScore = score;
                bestAction = action;
            }
        }

        if (bestAction != _currentAction)
        {
            _currentAction?.StopAction();
            _currentAction = bestAction;
            _currentAction?.StartAction();
        }

        if (_currentAction != null && _currentAction.CanBeUpdated)
        {
            _currentAction.UpdateAction(deltaFloat);
        }
    }

    public void OnSpawnComplete()
    {
        // Optional initialization after spawning
    }

    internal void SetState(BrainState state)
    {
        _state = state;
        if (_state != BrainState.Active)
        {
            _currentAction?.StopAction();
            _currentAction = null;
        }
        if (_state == BrainState.Dead)
        {
            // Disables frame processing in Godot (replaces Unity's enabled = false)
            SetProcess(false);
            SetPhysicsProcess(false);
        }
    }
}