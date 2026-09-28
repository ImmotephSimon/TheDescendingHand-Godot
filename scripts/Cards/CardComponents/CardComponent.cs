public abstract partial class CardComponent : Node3D
{
    protected IStatContainer Stats { get; private set; }
    protected CardRuntime Card { get; private set; }
    protected IEntity CardOwner { get; private set; }
    public virtual bool IsTicking => false;
    public IEnumerable<GameTag> GetTags() => _tags;
    private readonly GameTag[] _tags;

    protected CardComponent(params GameTag[] tags)
    {
        _tags = tags ?? Array.Empty<GameTag>();
    }


    public void Activate()
    {
        OnActivate();
    }

    public virtual void Initialize(CardRuntime card, IEntity cardOwner)
    {
        Card = card;
        CardOwner = cardOwner;
    }
    protected virtual void OnActivate() { }
    protected virtual void OnCancel() { }

    public void Cancel() => OnCancel();
}