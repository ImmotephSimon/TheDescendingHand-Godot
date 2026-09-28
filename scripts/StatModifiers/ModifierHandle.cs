public sealed class ModifierHandle
{
    public int Id { get; private set; }
    public GameTag Stat { get; private set; }

    public static readonly ModifierHandle Invalid = new(-1, null);
    public bool IsValid => Id != Invalid.Id;

    public ModifierHandle(int id, GameTag stat)
    {
        Id = id;
        Stat = stat;
    }

    internal void Invalidate()
    {
        Id = Invalid.Id;
        Stat = Invalid.Stat;
    }
}