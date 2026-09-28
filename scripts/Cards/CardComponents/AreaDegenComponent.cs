using Godot;

[GlobalClass]
public partial class AreaDegenComponent : CardComponent
{
    public AreaOverlapComponent Overlap { get; private set; }

    private DurationDamageComponent _degen;


    public void Configure(
        AreaOverlapComponent overlap,
        GameTag damageConversion,
        float effectiveness,
        float duration,
        int maxStacks)
    {
        Overlap = overlap;

        
        _degen.Configure(
            damageConversion,
            effectiveness,
            duration,
            maxStacks);

        Overlap.OnEntityEntered += OnEntered;
        Overlap.OnEntityExited += OnExited;
    }

    public override void Initialize(CardRuntime card, IEntity owner)
    {
        base.Initialize(card, owner);
        _degen = Card.AddCardComponent<DurationDamageComponent>();
        _degen.Initialize(card, owner);
    }

    protected override void OnActivate()
    {
        _degen.Activate();
    }

    private void OnEntered(IEntity target)
    {
        _degen.ApplyDegen(target);
    }

    private void OnExited(IEntity target)
    {
        _degen.StopDegen(target);
    }


    protected override void OnCancel()
    {
        _degen.Cancel();
        Overlap.Cancel();
    }
}