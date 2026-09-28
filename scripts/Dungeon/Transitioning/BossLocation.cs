[GlobalClass]
public partial class BossLocation : Node3D
{
    [Export] private FloorOpening floorOpening;
    [Export] private Node3D bossTransform;
    [Export] private Node3D stairsAnchor;

    private EnemySpawner spawner;

    public override void _Ready()
    {
        spawner = this.GetComponentInOwner<EnemySpawner>();
        if (spawner == null)
        {
            GD.PrintErr($"{Name}: BossLocation requires EnemySpawner.");
            return;
        }

        spawner.OnSpawned += HandleEnemySpawned;
    }

    private void HandleEnemySpawned(Enemy enemy)
    {
        spawner.OnSpawned -= HandleEnemySpawned;

        enemy.Died += OnEnemyDied;
    }

    private void OnEnemyDied(IEntity enemy)
    {
        enemy.Died -= OnEnemyDied;
        floorOpening?.OpenFloor();
        DungeonManager.Instance.OnDungeonCompleted(stairsAnchor);
    }
}