[GlobalClass]
public partial class LaunchDirection : Resource
{
    [Export] public float Sideways = 0f;
    [Export] public float Upward = 0f;
    [Export] public float Forward = 1f;
    [Export] public float Gravity = 0f;

    public LaunchDirection() { }

    public LaunchDirection(float sideways, float upward, float forward)
    {
        Sideways = sideways;
        Upward = upward;
        Forward = forward;
    }

    public Vector3 GetDirection(IEntity owner)
    {
        Transform3D transform = owner.Node.GlobalTransform;

        return (transform.Basis.X * Sideways) +
               (transform.Basis.Y * Upward) +
               (-transform.Basis.Z * Forward);
    }
}