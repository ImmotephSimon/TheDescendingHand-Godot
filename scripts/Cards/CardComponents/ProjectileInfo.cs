[GlobalClass]
public partial class ProjectileInfo : Resource
{
    [Export(PropertyHint.Range, "2,20")] public float Speed { get; set; } = 5f;
    [Export] public LaunchDirection Direction { get; set; }
    [Export] public PackedScene Visual { get; set; }

    [Export(PropertyHint.Range, "1,10")] public int Count { get; set; } = 1;
    [Export(PropertyHint.Range, "0,10")] public int Pierce { get; set; } = 0;
    [Export(PropertyHint.Range, "0,10")] public int Chains { get; set; } = 0;
    [Export(PropertyHint.Range, "0,10")] public int Bounces { get; set; } = 0;

    public Vector3 GetSpawnPosition(IEntity owner)
        => owner.Node.GlobalPosition + owner.Node.GlobalTransform.Basis.Y * 1.5f;

    public Vector3 GetLaunchVelocity(IEntity owner)
        => Direction.GetDirection(owner).Normalized() * Speed;

    public ProjectileInfo Clone() => (ProjectileInfo)Duplicate(true);
}