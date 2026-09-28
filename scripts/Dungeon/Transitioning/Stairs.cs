[GlobalClass]
public partial class Stairs : Node3D
{
    private enum State { Idle, TraversingDown, TraversingUp }
    private State _currentState = State.Idle;

    [Export] private Node3D _dungeonAnchor;
    [Export] private TriggerDetector _walkDownTrigger;
    [Export] private TriggerDetector _walkUpTrigger;
    [Export] private Godot.Collections.Array<Node3D> _walls = new();

    public override void _EnterTree()
    {
        if (_walkDownTrigger != null)
        {
            _walkDownTrigger.OnEntered += OnTopEnter;
            _walkDownTrigger.OnExited += OnTopExit;
        }

        if (_walkUpTrigger != null)
        {
            _walkUpTrigger.OnEntered += OnBottomEnter;
            _walkUpTrigger.OnExited += OnBottomExit;
        }
    }

    public override void _ExitTree()
    {
        if (_walkDownTrigger != null)
        {
            _walkDownTrigger.OnEntered -= OnTopEnter;
            _walkDownTrigger.OnExited -= OnTopExit;
        }

        if (_walkUpTrigger != null)
        {
            _walkUpTrigger.OnEntered -= OnBottomEnter;
            _walkUpTrigger.OnExited -= OnBottomExit;
        }
    }

    private void OnTopEnter(IEntity other)
    {
        if (_currentState != State.Idle) return;

        _currentState = State.TraversingDown;
        SetWallsActive(true);
        DungeonManager.Instance.EnterDungeon(_dungeonAnchor);
    }

    private void OnBottomExit(IEntity other)
    {
        if (_currentState == State.TraversingDown)
        {
            _currentState = State.Idle;
            SetWallsActive(false);
        }
    }

    private void OnBottomEnter(IEntity other)
    {
        if (_currentState != State.Idle) return;

        _currentState = State.TraversingUp;
        SetWallsActive(true);
        DungeonManager.Instance.LeaveDungeon();
    }

    private void OnTopExit(IEntity other)
    {
        if (_currentState == State.TraversingUp)
        {
            _currentState = State.Idle;
            SetWallsActive(false);
        }
    }

    private void SetWallsActive(bool active)
    {
        for (int i = 0; i < _walls.Count; i++)
        {
            if (_walls[i] != null) _walls[i].Visible = active;
        }
    }
}