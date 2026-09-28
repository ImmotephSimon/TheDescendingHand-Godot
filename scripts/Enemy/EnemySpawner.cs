[GlobalClass]
public partial class EnemySpawner : Node3D
{
    [Export] private PackedScene enemyPrefab;
    [Export] public EnemyDefinition enemyDefinition;
    [Export] private bool spawnOnEnter = true;
    [Export] private int spawnCount = 3;
    [Export] public Node3D fixedSpawnPoint;

    private Room room;
    private Aabb spawnBounds;

    public event Action<Enemy> OnSpawned;

    public override void _Ready()
    {
        room = this.GetComponentInParent<Room>();
        if (room == null)
        {
            GD.PrintErr("EnemySpawner requires a Room parent.");
            return;
        }

        room.Entered += SpawnEnemies;
    }

    private Vector3 GetRandomFloorPoint()
    {
        if (fixedSpawnPoint != null)
            return fixedSpawnPoint.GlobalPosition;

        float randomX = (float)GD.RandRange(spawnBounds.Position.X, spawnBounds.End.X);
        float randomZ = (float)GD.RandRange(spawnBounds.Position.Z, spawnBounds.End.Z);
        Vector3 spawnPoint = new Vector3(randomX, spawnBounds.End.Y + 5f, randomZ);

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(spawnPoint, spawnPoint + Vector3.Down * 20f);
        var result = spaceState.IntersectRay(query);

        if (result.Count > 0 && result.TryGetValue("position", out var hitVariant))
        {
            Vector3 hitPoint = (Vector3)hitVariant;
            Vector3 closestNavPoint = NavigationServer3D.MapGetClosestPoint(GetWorld3D().NavigationMap, hitPoint);
            if (closestNavPoint != Vector3.Zero)
            {
                return closestNavPoint + Vector3.Up * 0.1f;
            }
        }

        return Vector3.Zero;
    }

    public void Spawn(Vector3 point)
    {
        if (!Multiplayer.IsServer())
            return;

        Node3D obj = enemyPrefab.Instantiate<Node3D>();
        obj.GlobalPosition = point;

        Enemy enemy = obj.GetNodeOrNull<Enemy>(".");
        if (enemy != null)
        {
            enemy.Initialize(enemyDefinition, DungeonManager.Instance.ZoneLevel);
        }
        else
        {
            GD.PrintErr($"{obj.Name} has no Enemy component.");
            obj.QueueFree();
            return;
        }

        GetTree().CurrentScene.AddChild(obj);

        if (obj is ISpawnable spawnable)
        {
            spawnable.OnSpawnComplete();
        }

        OnSpawned?.Invoke(enemy);
    }

    public void SpawnEnemies()
    {
        if (!spawnOnEnter)
            return;

        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 point = GetRandomFloorPoint();
            if (point != Vector3.Zero)
                Spawn(point);
        }
    }
}