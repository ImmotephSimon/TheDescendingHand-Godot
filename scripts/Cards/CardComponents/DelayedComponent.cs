[GlobalClass]
public partial class DelayedComponent : CardComponent
{
    private float _delay;

    private float _timer;
    private bool _active;
    private bool _paused;
    public override bool IsTicking => true;
    public event Action OnCompleted;

    public void Configure(float delay)
    {
        _delay = delay;
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Start()
    {
        _paused = false;
    }

    public override void _Process(double delta)
    {
        if (!_active || _paused)
            return;

        _timer += (float)delta;

        if (_timer < _delay)
            return;

        _active = false;
        OnCompleted?.Invoke();
    }

    protected override void OnActivate()
    {
        _timer = 0f;
        _active = true;
        _paused = false;
    }
}