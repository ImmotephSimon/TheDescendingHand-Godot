[GlobalClass]
public partial class DamageAccComponent : CardComponent
{
    private readonly List<HitInfo> _accumulatedHits = new();
    private float _storedPct;

    public event Action<IReadOnlyList<HitInfo>, float> OnReleased;

    public void Configure(float storedPct) => _storedPct = storedPct;

    public void HandleHit(HitInfo info) => _accumulatedHits.Add(info);

    public void Release()
    {
        if (_accumulatedHits.Count > 0)
        {
            float totalScalar = _storedPct * _accumulatedHits.Count;
            OnReleased?.Invoke(_accumulatedHits, totalScalar);
        }
        _accumulatedHits.Clear();
    }

    protected override void OnActivate()
    {
        _accumulatedHits.Clear();
        if (Card != null) Card.OnHit += HandleHit;
    }

    protected override void OnCancel()
    {
        if (Card != null) Card.OnHit -= HandleHit;
        _accumulatedHits.Clear();
    }
}