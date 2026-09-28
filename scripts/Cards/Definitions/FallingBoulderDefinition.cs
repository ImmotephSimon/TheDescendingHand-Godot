[GlobalClass]
public partial class FallingBoulderDefinition : CardDefinition
{
    [Export] private PackedScene _boulderPrefab;
    [Export] private float _effectiveness;
    [Export] private GameTag _damageConversion;
    [Export] private float _radius = 3f;
    [Export] private Vector3 _spawnOffset = new(0f, 10f, 0f);

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var spawn = card.AddCardComponent<MeshSpawnComponent>();
        spawn.Configure(_boulderPrefab, _spawnOffset, useGravity: true);

        var overlap = card.AddSphereOverlap(_radius);
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage.Configure(_effectiveness, _damageConversion);

        spawn.OnSpawned += (spawnedNode) =>
        {
            // Set the collision layer if the spawned node is a physics object
            if (spawnedNode is CollisionObject3D collisionObject)
            {
                collisionObject.CollisionLayer = (uint)card.CardOwner.AttackLayer;
            }

            overlap.Attach(spawnedNode);

            // Ensure FractureComponent exists on the spawned instance
            var fracture = spawnedNode.GetComponentInOwner<FractureComponent>();
            if (fracture == null)
            {
                fracture = new FractureComponent();
                spawnedNode.AddChild(fracture);
            }
        };
    }
}