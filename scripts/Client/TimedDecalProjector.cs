using Godot;

[GlobalClass]
public partial class TimedDecalProjector : Node3D, IVfx
{
    [Export] private Decal _decalProjector;

    private float _fadeInMid = 0.8f;
    private float _fadeInEnd = 1.0f;
    private float _fadeOutDuration = 0.3f;

    private float _duration;
    private float _elapsed;
    private bool _isRunning;

    public override void _Ready()
    {
        _decalProjector ??= GetNodeOrNull<Decal>("Decal") ?? GetChild<Decal>(0);
    }

    public virtual void Initialize(VfxSpawnParams spawnParams, Node3D target = null)
    {

        // Sets both position and rotation in a single transform call
        GlobalTransform = new Transform3D(new Basis(spawnParams.Rotation), spawnParams.Position);

        if (_decalProjector != null && spawnParams.Scale != Vector3.Zero)
        {
            _decalProjector.Size = spawnParams.Scale;
        }

        _duration = spawnParams.Duration;
        _elapsed = 0f;
        _isRunning = true;

        SetProgress(0f);
    }

    public override void _Process(double delta)
    {
        if (!_isRunning)
            return;

        _elapsed += (float)delta;
        SetProgress(_elapsed / _duration);
    }

    private void SetProgress(float progress)
    {
        if (_decalProjector == null)
            return;

        float fade;

        if (progress < _fadeInMid)
        {
            fade = Mathf.Lerp(0.1f, 0.2f, progress / _fadeInMid);
        }
        else if (progress < _fadeInEnd)
        {
            float t = Mathf.InverseLerp(_fadeInMid, _fadeInEnd, progress);
            fade = Mathf.Lerp(0.2f, 1.0f, t);
        }
        else
        {
            float t = (progress - _fadeInEnd) / _fadeOutDuration;
            fade = Mathf.Lerp(1.0f, 0.0f, t);

            if (t >= 1.0f)
            {
                Stop();
                return;
            }
        }

        _decalProjector.AlbedoMix = fade;
    }

    public virtual void Stop()
    {
        _isRunning = false;
        QueueFree();
    }

    public virtual void UpdateTarget(Node3D target) { }
}