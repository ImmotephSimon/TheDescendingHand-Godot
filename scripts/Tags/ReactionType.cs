[GlobalClass]
public abstract partial class ReactionType : Resource
{
    protected abstract GameTag GetListeningTag();

    public virtual void StartListening(IEntity owner)
    {
        var tag = GetListeningTag();

        if (tag == null) return;

        ModifierHandle handle = null;

        void OnStatChanged(float newValue)
        {
            if (newValue > 0 && handle == null)
            {
                handle = Apply(owner, newValue);
            }
            else if (newValue <= 0 && handle != null)
            {
                owner.Stats.RemoveModifier(handle);
                handle = null;
            }
        }

        owner.Stats.Listen(tag, OnStatChanged);

        OnStatChanged(owner.Stats.GetStat(tag));
    }

    protected abstract ModifierHandle Apply(IEntity owner, float newValue);
}