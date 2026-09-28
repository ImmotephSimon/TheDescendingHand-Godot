[GlobalClass]
public partial class DropsComponent : Node3D
{
    private static readonly Random _rng = new();

    public void DropFromChest(Vector3 position, Vector3 forward)
    {
        Rarity rarity = GameData.Instance.Items.RollRandomRarity(_rng, minimumTier: 1);
        LootDefinition definition = rarity.AllowedDrops[_rng.Next(rarity.AllowedDrops.Count)];

        Vector3 spawnPosition = GetFloorPosition(position) + Vector3.Up * definition.DropHeight;
        Vector3 force = Vector3.Up * definition.UpForce + forward * definition.ForwardForce;

        Drop(definition, rarity, spawnPosition, force);
    }

    public void DropFromEnemy(Vector3 position)
    {
        Rarity rarity = GameData.Instance.Items.RollRandomRarity(_rng);
        LootDefinition definition = rarity.AllowedDrops[_rng.Next(rarity.AllowedDrops.Count)];

        float angle = (float)(_rng.NextDouble() * Math.PI * 2);
        Vector2 dir = new Vector2(MathF.Cos(angle), MathF.Sin(angle));

        Vector3 force = new Vector3(dir.X, 1f, dir.Y) * (definition.UpForce + (float)(_rng.NextDouble() * 2f - 1f));
        Vector3 spawnPosition = GetFloorPosition(position);

        Drop(definition, rarity, spawnPosition, force);
    }

    private Vector3 GetFloorPosition(Vector3 position)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(position, position + Vector3.Down * 1000f);
        query.CollisionMask = 1u << 1; // Floor layer

        var result = spaceState.IntersectRay(query);
        if (result.Count > 0)
        {
            Vector3 hitPoint = (Vector3)result["position"];
            return hitPoint;
        }

        GD.PrintErr($"Could not find floor below drop position {position}.");
        return position;
    }

    private void Drop(LootDefinition definition, Rarity rarity, Vector3 position, Vector3 force)
    {
        var obj = definition.Prefab.Instantiate<Node3D>();
        obj.GlobalPosition = position;
        GetTree().CurrentScene.AddChild(obj);

        WorldDrop drop = obj.GetComponentInOwner<WorldDrop>();

        if (obj is RigidBody3D rb)
        {
            rb.ApplyCentralImpulse(force);
            if (definition.Torque != 0f)
            {
                Vector3 randomTorque = new Vector3(
                    (float)(_rng.NextDouble() * 2f - 1f),
                    (float)(_rng.NextDouble() * 2f - 1f),
                    (float)(_rng.NextDouble() * 2f - 1f)
                ).Normalized() * definition.Torque;

                rb.ApplyTorqueImpulse(randomTorque);
            }
        }

        definition.Initialize(drop, rarity);
        GD.Print($"Dropping {definition.Prefab.ResourceName}[{rarity.ResourceName}]");
    }
}