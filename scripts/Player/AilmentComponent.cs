[GlobalClass]
public partial class AilmentComponent : Node3D, IAilmentHandler
{
    private float poiseMultiplier = 2;
    private float stunImmunityDuration = 2f;

    private float StunThreshold => _stats.GetStat(GameTags.ModStatStunThreshold);
    private float StunDuration => _stats.GetStat(GameTags.ModStatStunDuration);

    private IStatContainer _stats;
    private IStunnable _stunnable;
    private float _poise;
    private float _stunImmunityTimer;

    public override void _Ready()
    {
        _stats = this.GetComponentInOwner<IStatContainer>();
        _stunnable = this.GetComponentInOwner<IStunnable>();
    }

    public override void _Process(double delta)
    {
        if (_stunImmunityTimer > 0)
            _stunImmunityTimer -= (float)delta;
    }

    public void ApplyAilments(DamageInfo info)
    {
        CalcStun(info);
        ApplyDamageAilments(info);
    }

    private void CalcStun(DamageInfo info)
    {
        info.DamageMap.TryGetValue(GameTags.RestrictionPhysical, out float damage);

        _poise += damage;

        float maxHealth = _stats.GetStat(GameTags.ModStatHealth);

        if (_stunImmunityTimer > 0)
            return;

        if (damage >= maxHealth * StunThreshold || _poise >= maxHealth * StunThreshold * poiseMultiplier)
        {
            ApplyStun(damage);
        }
    }

    private void ApplyStun(float damage)
    {
        _poise = 0;
        _stunImmunityTimer = stunImmunityDuration;

        _stunnable.ApplyStun(StunDuration);
    }

    private void ApplyDamageAilments(DamageInfo info)
    {
        if (info.Tags.HasTag(GameTags.ModSpecialColdDamageCanIgnite))
            ApplyIgnite(info);

        if (info.Tags.HasTag(GameTags.ModSpecialPoisonDamageCanFreeze))
            ApplyFreeze(info);
    }

    private void ApplyFreeze(DamageInfo info) => throw new NotImplementedException();
    private void ApplyIgnite(DamageInfo info) => throw new NotImplementedException();
}