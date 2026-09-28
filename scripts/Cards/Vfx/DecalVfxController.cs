using Godot;
using System;

[GlobalClass]
public partial class DecalVfxController : Node3D, IVfx
{
    [Export] private Decal _decal;
    [Export] private GpuParticles3D _vfx;
    [Export] private float _lightningInterval = 1f;
    [Export] private float _creepSpeed = 2f;

    private static readonly StringName Pos1Param = "shader_parameter/Pos1";
    private static readonly StringName Pos4Param = "shader_parameter/Pos4";

    private float _currentTime;
    private float _nextLightningTime;
    private Vector3 _currentStart;
    private Vector3 _currentEnd;
    private Vector3 _targetEnd;
    private bool _isInitialized;

    public void Initialize(VfxSpawnParams spawnParams, Node3D target = null)
    {
        if (_decal == null || _vfx == null)
            throw new InvalidOperationException("DecalVfxController requires a Decal and VFX node.");

        _currentTime = 0f;
        _nextLightningTime = 0f;
        _isInitialized = false;
    }

    public void Stop()
    {
        _vfx.Emitting = false;
        _isInitialized = false;
    }

    public void UpdateTarget(Node3D target)
    {
        throw new NotImplementedException();
    }

    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _currentTime += dt;

        if (_currentTime >= _nextLightningTime)
        {
            _nextLightningTime = _currentTime + _lightningInterval;
            OnIntervalTick();
            _vfx.Emitting = true;
            _vfx.Restart();
        }

        if (_isInitialized)
        {
            _currentStart = _currentStart.MoveToward(_currentEnd, _creepSpeed * dt);

            if (_currentStart.IsEqualApprox(_currentEnd))
            {
                _currentEnd = _targetEnd;
            }

            SetVfxVector3(Pos1Param, _currentStart);
            SetVfxVector3(Pos4Param, _targetEnd);
        }
    }

    private void OnIntervalTick()
    {
        Vector3 newPoint = GetRandomBoundaryPoint();

        if (!_isInitialized)
        {
            _currentStart = newPoint;
            _currentEnd = GetRandomBoundaryPoint();
            _targetEnd = _currentEnd;
            _isInitialized = true;
        }
        else
        {
            _targetEnd = newPoint;
        }
    }

    private Vector3 GetRandomBoundaryPoint()
    {
        Vector3 decalSize = _decal.Size;
        Vector2 halfSize = new Vector2(decalSize.X, decalSize.Z) * 0.5f;

        float angle = (float)GD.RandRange(0, Mathf.Tau);
        Vector2 p = new Vector2(Mathf.Cos(angle) * halfSize.X, Mathf.Sin(angle) * halfSize.Y);

        return _decal.ToGlobal(new Vector3(p.X, 0f, p.Y));
    }

    private void SetVfxVector3(StringName param, Vector3 value)
    {
        if (_vfx.ProcessMaterial is ShaderMaterial mat)
        {
            mat.SetShaderParameter(param, value);
        }
    }
}