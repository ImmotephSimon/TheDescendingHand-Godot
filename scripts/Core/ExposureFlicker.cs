[GlobalClass]
public partial class ExposureFlicker : Node
{
    [Export] private float _minEnergy = 0.85f;
    [Export] private float _maxEnergy = 1.05f;
    [Export] private float _speed = 15f;
    [Export] private Light3D _targetLight;

    private readonly FastNoiseLite _noise = new();
    private float _time;

    public override void _Ready()
    {
        _noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Perlin;
        _targetLight ??= GetNodeOrNull<Light3D>("OmniLight3D");
    }

    public override void _Process(double delta)
    {
        if (_targetLight == null) return;

        _time += (float)delta * _speed;
        float sample = (_noise.GetNoise1D(_time) + 1f) * 0.5f; // Normalize -1..1 to 0..1
        _targetLight.LightEnergy = Mathf.Lerp(_minEnergy, _maxEnergy, sample);
    }
}