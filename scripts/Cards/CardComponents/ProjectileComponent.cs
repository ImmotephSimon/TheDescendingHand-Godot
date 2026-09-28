[GlobalClass]
public partial class ProjectileComponent : CardComponent
{
    private ProjectileInfo _info;
    private Func<Node3D, Node3D> _onSpawnProjectile;
    private const float MultiProjectileAngle = 15f;

    public event Action<HitInfo> OnProjectileHit;
    public event Action<ProjectileController> OnSpawned;

    public void Configure(
        ProjectileInfo info,
        Func<Node3D, Node3D> spawnNetworkObject)
    {
        _info = info;
        _onSpawnProjectile = spawnNetworkObject;
    }

    protected override void OnActivate()
    {
        for (int i = 0; i < _info.Count; i++)
        {
            float angle = GetSpreadAngle(i, _info.Count);

            Quaternion rotation = CardOwner.Node.Quaternion * Quaternion.FromEuler(new Vector3(0, Mathf.DegToRad(angle), 0));

            SpawnProjectile(rotation);
        }
    }

    private float GetSpreadAngle(int index, int count)
    {
        if (count == 1)
            return 0f;

        float t = (float)index / (count - 1);

        return Mathf.Lerp(-MultiProjectileAngle, MultiProjectileAngle, t);
    }

    private void SpawnProjectile(Quaternion rotation)
    {
        if (!GameData.Instance.ServerPrefabs.TryGetPrefab(GameTags.PrefabProjectile, out PackedScene prefab))
            return;

        var projectile = prefab.Instantiate<Node3D>();
        projectile.Position = _info.GetSpawnPosition(CardOwner);
        projectile.Quaternion = rotation;

        var controller = projectile.GetComponentInChildren<ProjectileController>();
        if (controller != null)
        {
            controller.OnHit += hit =>
            {
                OnProjectileHit?.Invoke(hit);
                Card.OnHit(hit);
            };
            controller.Initialize(_info.Clone(), CardOwner);
        }

        _onSpawnProjectile?.Invoke(projectile);
        OnSpawned?.Invoke(controller);
    }
}