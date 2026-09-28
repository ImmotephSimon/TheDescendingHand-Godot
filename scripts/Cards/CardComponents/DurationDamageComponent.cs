using System.Collections.Generic;

[GlobalClass]
public partial class DurationDamageComponent : CardComponent
{
    private ICalculator _calc;
    private float _effectiveness;
    private float _duration;
    private int _maxStacks;

    private Dictionary<GameTag, float> _damage;
    private GameTag _damageConversion;

    public void Configure(
        GameTag damageConversion,
        float effectiveness,
        float duration,
        int maxStacks)
    {
        _damageConversion = damageConversion;
        _effectiveness = effectiveness;
        _duration = duration;
        _maxStacks = maxStacks;
    }

    public override void Initialize(CardRuntime card, IEntity owner)
    {
        base.Initialize(card, owner);

        _calc = CardOwner.Node.GetComponentInOwner<ICalculator>();
        Debug.Assert(_calc != null, $"Failed to find calculator.");

        Card.OnHit += HandleHit;
    }

    public void HandleHit(HitInfo info)
    {
        if (info.Target is IDamageable damageable)
        {
            var degenInfo = new DegenInfo(
                Card.Id,
                _damage,
                info.Source,
                info.Position,
                _duration,
                _maxStacks);

            damageable.ApplyDegen(degenInfo);
        }
    }

    public void ApplyDegen(IEntity target)
    {
        if (target is IDamageable damageable)
        {
            var degenInfo = new DegenInfo(
                Card.Id,
                _damage,
                CardOwner,
                target.Node.Position,
                _duration,
                _maxStacks);
            damageable.ApplyDegen(degenInfo);
        }
    }

    public void StopDegen(IEntity target)
    {
        if (target is IDamageable damageable)
        {
            damageable.RemoveDegen(Card.Id); 
        }
    }

    protected override void OnActivate()
    {
        _damage = _calc.CalculateDamage(Card.Tags, _effectiveness, _damageConversion);
    }
}