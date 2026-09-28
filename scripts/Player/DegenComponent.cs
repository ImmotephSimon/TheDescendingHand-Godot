[GlobalClass]
public partial class DegenComponent : Node
{
    private const float TickInterval = 0.2f;
    private float _tickTimer;
    private IHealth _healthHandler;
    private readonly List<ActiveDegen> _activeDegens = new();

    private class ActiveDegen
    {
        public DegenInfo Info { get; }
        public float ExpirationTime { get; }

        public ActiveDegen(DegenInfo info)
        {
            Info = info;
            ExpirationTime = (float)(Godot.Time.GetTicksMsec() / 1000.0) + info.Duration;
        }
    }

    public override void _Ready()
    {
        _healthHandler = this.GetComponentInOwner<IHealth>();
    }

    public void Apply(DegenInfo degenInfo)
    {
        int count = 0;
        ActiveDegen oldest = null;

        for (int i = 0; i < _activeDegens.Count; i++)
        {
            if (_activeDegens[i].Info.Id != degenInfo.Id)
                continue;

            count++;

            if (oldest == null || _activeDegens[i].ExpirationTime < oldest.ExpirationTime)
                oldest = _activeDegens[i];
        }

        if (count < degenInfo.MaxStacks)
        {
            _activeDegens.Add(new ActiveDegen(degenInfo));
        }
        else if (oldest != null)
        {
            _activeDegens.Remove(oldest);
            _activeDegens.Add(new ActiveDegen(degenInfo));
        }
    }

    public void RemoveDegen(Guid id)
    {
        _activeDegens.RemoveAll(d => d.Info.Id == id);
    }

    public override void _Process(double delta)
    {
        if (_activeDegens.Count == 0)
            return;

        _tickTimer += (float)delta;

        if (_tickTimer < TickInterval)
            return;

        _tickTimer = 0f;

        var activeDegens = _activeDegens.ToArray();
        float currentTime = (float)(Godot.Time.GetTicksMsec() / 1000.0);

        foreach (var degen in activeDegens)
        {
            foreach (var (tag, amount) in degen.Info.Damage)
            {
                _healthHandler.AdjustHealth(
                    -amount * TickInterval,
                    degen.Info.Source,
                    false);
            }

            if (currentTime >= degen.ExpirationTime)
                _activeDegens.Remove(degen);
        }
    }
}