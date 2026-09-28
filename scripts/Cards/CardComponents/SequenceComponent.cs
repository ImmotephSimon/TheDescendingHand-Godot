using System.Threading.Tasks;

[GlobalClass]
public partial class SequenceComponent : CardComponent
{
    private int _steps;
    private float _delay;

    public Action OnSequence;

    public void Configure(int steps, float delay)
    {
        _steps = steps;
        _delay = delay;
    }

    public override void Initialize(CardRuntime card, IEntity owner)
    {
        base.Initialize(card, owner);
        _ = RunSequenceAsync();
    }

    private async Task RunSequenceAsync()
    {
        for (int i = 0; i < _steps; i++)
        {
            OnSequence?.Invoke();
            if (i < _steps - 1 && _delay > 0f)
            {
                await ToSignal(GetTree().CreateTimer(_delay), SceneTreeTimer.SignalName.Timeout);
            }
        }
    }
}