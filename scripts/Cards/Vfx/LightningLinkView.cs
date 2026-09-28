[GlobalClass]
public partial class ChainLightningView : Node3D, IVfx
{
    [Export] private GpuParticles3D _visualEffect;

    private enum LightningState
    {
        Grow,
        Fixed,
        Shrink
    }

    private static readonly StringName Pos1Param = "shader_parameter/Pos1";
    private static readonly StringName Pos4Param = "shader_parameter/Pos4";

    private float _minShrinkTime = 1f;
    private float _maxLength = 10f;
    private Vector3 _previousEnd;
    private float _shrinkSpeed;
    private LightningState _state = LightningState.Grow;
    private Vector3 _start;
    private Vector3 _end;
    private float _maxSpeed;
    private Vector3 _originalStart;

    private Node3D _target;
    private float _shrinkTimer;

    public override void _Ready()
    {
        _visualEffect ??= this.GetComponentInOwner<GpuParticles3D>();

        if (_visualEffect == null)
        {
            GD.PrintErr("Missing GpuParticles3D component on ChainLightningView.", this);
        }
    }

    public void Initialize(VfxSpawnParams spawnParams, Node3D target = null)
    {
        _originalStart = spawnParams.Position;
        _start = spawnParams.Position;
        _target = target;
        _end = target != null ? target.GlobalPosition : spawnParams.Position;
        _previousEnd = _end;

        SetVfxVector3(Pos1Param, _start);
        SetVfxVector3(Pos4Param, _end);

        GD.Print($"[VFX Init] Start: {_start}, End: {_end}, TargetNull: {target == null}, TargetPos: {target?.GlobalPosition}");
    }

    public void UpdateTarget(Node3D target)
    {
        _target = target;
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;

        if (_target == null)
        {
            _state = LightningState.Shrink;
        }
        else
        {
            UpdateSpeed(dt);
        }

        switch (_state)
        {
            case LightningState.Grow:
                Grow();
                break;

            case LightningState.Fixed:
                _start = _start.MoveToward(_end, _maxSpeed * dt);
                break;

            case LightningState.Shrink:
                Shrink(dt);
                break;
        }

        SetVfxVector3(Pos1Param, _start);
        SetVfxVector3(Pos4Param, _end);
    }

    private void UpdateSpeed(float dt)
    {
        if (dt > 0f && IsInstanceValid(_target))
        {
            _end = _target.GlobalPosition;
            float currentSpeed = _previousEnd.DistanceTo(_end) / dt;
            _maxSpeed = Mathf.Max(_maxSpeed, currentSpeed);
            _previousEnd = _end;
        }
    }

    private void Grow()
    {
        if (_start.DistanceTo(_end) < 0.1f || _originalStart.DistanceTo(_end) >= _maxLength)
        {
            _state = LightningState.Fixed;
        }
    }

    private void Shrink(float dt)
    {
        _start = _start.MoveToward(_end, _maxSpeed * dt);
        _shrinkTimer -= dt;

        if (_start.IsEqualApprox(_end) && _shrinkTimer <= 0f)
        {
            QueueFree();
        }
    }

    public void Stop()
    {
        GD.Print($"Stopping VFX {Name}");
        if (IsInstanceValid(_target))
        {
            _end = _target.GlobalPosition;
            _target = null;
        }

        // Reparent to root so it persists during collapse if parent is destroyed
        if (GetParent() != null)
        {
            Reparent(GetTree().Root);
        }

        float distance = _start.DistanceTo(_end);
        float naturalTime = _maxSpeed > 0f ? distance / _maxSpeed : 0f;

        _shrinkTimer = Mathf.Max(_minShrinkTime, naturalTime);
        _state = LightningState.Shrink;
    }

    private void SetVfxVector3(StringName param, Vector3 value)
    {
        if (_visualEffect != null && _visualEffect.ProcessMaterial is ShaderMaterial mat)
        {
            mat.SetShaderParameter(param, value);
        }
    }
}