[GlobalClass]
public partial class SuctionComponent : CardComponent
{
    private readonly float _radius;
    private readonly float _pullForce;
    private readonly uint _enemyLayer;

    private bool _isSuctionActive;

    public SuctionComponent(
        float radius,
        float pullForce,
        uint enemyLayer)
    {
        _radius = radius;
        _pullForce = pullForce;
        _enemyLayer = enemyLayer;
    }

    protected override void OnActivate()
    {
        _isSuctionActive = true;
    }

    public override void _Process(double delta)
    {
        if (!_isSuctionActive)
            return;

        var spaceState = GetWorld3D().DirectSpaceState;

        var shape = new SphereShape3D { Radius = _radius };
        var query = new PhysicsShapeQueryParameters3D
        {
            Shape = shape,
            Transform = new Transform3D(Basis.Identity, CardOwner.Node.GlobalPosition),
            CollisionMask = _enemyLayer
        };

        var results = spaceState.IntersectShape(query);

        foreach (var result in results)
        {
            if (result["collider"].As<Node3D>() is RigidBody3D rb)
            {
                Vector3 direction = CardOwner.Node.GlobalPosition - rb.GlobalPosition;

                if (direction.LengthSquared() > 0.01f)
                {
                    rb.ApplyCentralForce(direction.Normalized() * _pullForce);
                }
            }
        }
    }
}