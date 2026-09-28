
[GlobalClass]
public partial class DirectDamageComponent : CardComponent
{
    private Dictionary<GameTag, float> _damage = new();
    private GameTag _damageConversion;
    private bool _triggerOnHit;
    private float _effectiveness;
    private ICalculator _calc;

    public void Configure(float effectiveness, GameTag damageConversion, bool triggerOnHit = true)
    {
        _effectiveness = effectiveness;
        _damageConversion = damageConversion;
        _triggerOnHit = triggerOnHit;

        if (_triggerOnHit)
            Card.OnHit += HandleHit;
    }

    public override void Initialize(CardRuntime card, IEntity owner)
    {
        base.Initialize(card, owner);

        _calc = Card.CardOwner.Node.GetComponentInChildren<ICalculator>();
        Debug.Assert(_calc != null, $"Failed to find calculator.");

    }

    private void HandleHit(HitInfo info)
    {
        DamageInfo damageInfo = new(
            _damage,
            info.Source,
            info.Position
        );

        if (info.Target is IDamageable damageable)
        {
            damageable.TakeDamage(damageInfo);
        }
    }

    public void TriggerDamage(HitInfo hit, float scalar = 1f)
    {
        var damage = _damage.ToDictionary(x => x.Key, x => x.Value * scalar);

        if (hit.Target is IDamageable damageable)
            damageable.TakeDamage(new DamageInfo(damage, hit.Source, hit.Position));
    }


    protected override void OnActivate()
    {
        _damage = _calc.CalculateDamage(Card.Tags, _effectiveness, _damageConversion);
    }

}