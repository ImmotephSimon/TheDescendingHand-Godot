public abstract partial class ItemUseComponent : Resource
{
    public virtual void Initialize(ItemDropInstance instance) { }
    public abstract void Use(ItemDropInstance instance, IEntity user);
}