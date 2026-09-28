using System.Threading.Tasks;

[GlobalClass]
public partial class FirewallVFX : Node3D, IVfx
{
    [Export] private GpuParticles3D leftWaveVfx;
    [Export] private GpuParticles3D leftTrailVfx;
    [Export] private GpuParticles3D rightWaveVfx;
    [Export] private GpuParticles3D rightTrailVfx;

    [Export] private PackedScene flamePrefab;
    [Export] private int flamesPerSide = 3;
    [Export] private float distanceBetweenFlames = 1.5f;
    [Export] private float initialDelay = 0f;

    private int _spawnRoutineToken;

    public override void _Ready()
    {
        rightWaveVfx?.Set(Node3D.PropertyName.RotationDegrees, new Vector3(0, -90, 0));
        leftWaveVfx?.Set(Node3D.PropertyName.RotationDegrees, new Vector3(0, 90, 0));

        SetTrailDistance(leftTrailVfx);
        SetTrailDistance(rightTrailVfx);

        SetMainVelocity(leftWaveVfx);
        SetMainVelocity(rightWaveVfx);
    }

    public void Initialize(VfxSpawnParams spawnParams, Node3D target = null)
    {
        _spawnRoutineToken++;

        GlobalTransform = spawnParams.ToTransform3D();

        float lifetime = leftWaveVfx != null ? (float)leftWaveVfx.Lifetime : 1.0f;
        float stepDelay = lifetime / flamesPerSide;

        _ = SpawnFirewall(stepDelay, spawnParams.Duration, _spawnRoutineToken);
    }

    public void Stop()
    {
        _spawnRoutineToken++;
        QueueFree();
    }

    public void UpdateTarget(Node3D target)
    {
    }

    private async Task SpawnFirewall(float stepDelay, float duration, int token)
    {
        if (initialDelay > 0f)
        {
            await ToSignal(GetTree().CreateTimer(initialDelay), SceneTreeTimer.SignalName.Timeout);
            if (token != _spawnRoutineToken || !IsInstanceValid(this)) return;
        }

        SpawnFlame(GlobalPosition);

        for (int i = 1; i <= flamesPerSide; i++)
        {
            if (stepDelay > 0f)
            {
                await ToSignal(GetTree().CreateTimer(stepDelay), SceneTreeTimer.SignalName.Timeout);
                if (token != _spawnRoutineToken || !IsInstanceValid(this)) return;
            }

            float offset = i * distanceBetweenFlames;
            Vector3 rightOffset = GlobalTransform.Basis.X * offset;

            SpawnFlame(GlobalPosition + rightOffset);
            SpawnFlame(GlobalPosition - rightOffset);
        }

        float elapsedSpawnTime = (flamesPerSide + 1) * stepDelay + initialDelay;
        float remainingDuration = duration - elapsedSpawnTime;

        if (remainingDuration > 0f)
        {
            await ToSignal(GetTree().CreateTimer(remainingDuration), SceneTreeTimer.SignalName.Timeout);
            if (token != _spawnRoutineToken || !IsInstanceValid(this)) return;
        }

        Stop();
    }

    private void SpawnFlame(Vector3 position)
    {
        if (flamePrefab == null) return;

        var instance = flamePrefab.Instantiate<Node3D>();
        AddChild(instance);
        instance.GlobalPosition = position;
        instance.GlobalRotation = GlobalRotation;
    }

    private void SetTrailDistance(GpuParticles3D vfx)
    {
        if (vfx?.ProcessMaterial is ParticleProcessMaterial mat)
        {
            mat.OrbitVelocityMin = 2f;
            mat.OrbitVelocityMax = 2f;
        }
    }

    private void SetMainVelocity(GpuParticles3D vfx)
    {
        if (vfx == null) return;

        vfx.Lifetime = 1f;
        if (vfx.ProcessMaterial is ParticleProcessMaterial mat)
        {
            mat.Direction = new Vector3(0, 0, 1);
            mat.InitialVelocityMin = 4f;
            mat.InitialVelocityMax = 4f;
        }
    }
}