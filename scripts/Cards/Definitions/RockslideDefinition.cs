using Godot;

[GlobalClass]
public partial class RockslideDefinition : CardDefinition
{
    [Export] public PackedScene RockPrefab { get; set; }
    [Export] public float Effectiveness { get; set; }
    [Export] public GameTag DamageConversion { get; set; }
    [Export] public float Radius { get; set; } = 1f;
    [Export] public Vector3 SpawnOffset { get; set; } = new Vector3(0f, 10f, 0f);
    [Export] public float Spread { get; set; } = 1.5f;
    [Export] public int Count { get; set; } = 6;
    [Export] public float Interval { get; set; } = 0.5f;
    [Export] public float MeshScale { get; set; } = 0.3f;

    public override void Construct(CardInitContext context, CardRuntime card)
    {
        var damage = card.AddCardComponent<DirectDamageComponent>();
        damage?.Configure(Effectiveness, DamageConversion);

        var spawner = card.AddCardComponent<MeshSpawnComponent>();
        if (spawner != null)
        {
            spawner.Configure(
                RockPrefab,
                SpawnOffset,
                useGravity: true,
                scale: MeshScale);

            spawner.OnSpawned += spawnedNode =>
            {
                if (spawnedNode is not Node3D rockNode) return;

                // Random 2D offset in XZ plane
                float angle = (float)GD.RandRange(0, Mathf.Tau);
                float dist = (float)GD.RandRange(0, Spread);
                rockNode.GlobalPosition += new Vector3(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);

                // Random 3D rotation (Godot Euler angles use radians)
                rockNode.Quaternion = Quaternion.FromEuler(new Vector3(
                    (float)GD.RandRange(0, Mathf.Tau),
                    (float)GD.RandRange(0, Mathf.Tau),
                    (float)GD.RandRange(0, Mathf.Tau)
                ));

                // Attach trigger collider & overlap detector
                var area = rockNode.GetComponentInOwner<AreaOverlapComponent>() ?? new AreaOverlapComponent();
                if (area.GetParent() == null) rockNode.AddChild(area);

                var collisionShape = new CollisionShape3D { Shape = new SphereShape3D { Radius = Radius } };
                area.AddChild(collisionShape);

                area.OnEntityEntered += target =>
                {
                    damage?.TriggerDamage(new HitInfo(
                        target,
                        card.CardOwner,
                        target.Node.GlobalPosition));
                };
            };
        }

        var sequence = card.AddCardComponent<SequenceComponent>();
        sequence.Configure(Count, Interval);
        sequence.OnSequence += () => spawner?.Activate();
    }
}