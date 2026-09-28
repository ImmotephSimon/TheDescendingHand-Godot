

public struct HitInfo
{
    public IEntity Target;
    public IEntity Source;
    public Vector3 Position;

    public HitInfo(IEntity target, IEntity source, Vector3 position)
    {
        Target = target;
        Source = source;
        Position = position;
    }
}