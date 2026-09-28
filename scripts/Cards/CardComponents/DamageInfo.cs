public class DamageInfo
{
    public Dictionary<GameTag, float> DamageMap;
    public IEntity Source;
    public Vector3 HitPosition;

    public DamageInfo(
        Dictionary<GameTag, float> damageMap,
        IEntity source,
        Vector3 hitPosition)
    {
        DamageMap = damageMap;
        Source = source;
        HitPosition = hitPosition;
    }

    public TagContainer Tags => TagContainer.Empty;
}