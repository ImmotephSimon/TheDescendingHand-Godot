public interface IVfx
{
    void Initialize(VfxSpawnParams spawnParams, Node3D target = null);
    void Stop();
    void UpdateTarget(Node3D target);
}