using Godot;

[GlobalClass]
public partial class BossHealthBar : Node3D, IHealthBar
{
    private TextureProgressBar _fill;
    private IHealth _health;

    private float _displayedHealth;
    private float _targetHealth;
    private float _degenStartHealth;
    private float _degenTimer;
    private const float DegenDuration = 0.25f;

    public override void _Ready()
    {
        _fill = GetNodeOrNull<TextureProgressBar>("Fill") ?? this.GetComponentInChildren<TextureProgressBar>(true);
    }

    public void Bind(IHealth health)
    {
        _health = health;
        _health.OnHealthChanged += UpdateHealth;

        _displayedHealth = _health.CurrentHealth;
        _targetHealth = _health.CurrentHealth;

        if (_fill != null && _health.MaxHealth > 0)
        {
            _fill.MaxValue = _health.MaxHealth;
            _fill.Value = _displayedHealth;
        }
    }

    public void Unbind()
    {
        if (_health == null) return;
        _health.OnHealthChanged -= UpdateHealth;
        _health = null;
    }

    private void UpdateHealth(float current, float max, bool isInstant)
    {
        if (isInstant)
        {
            _displayedHealth = current;
            _targetHealth = current;
            if (_fill != null && max > 0)
            {
                _fill.MaxValue = max;
                _fill.Value = current;
            }
        }
        else
        {
            _degenStartHealth = _displayedHealth;
            _targetHealth = current;
            _degenTimer = 0f;
        }
    }

    public override void _Process(double delta)
    {
        if (_health == null || Mathf.IsEqualApprox(_displayedHealth, _targetHealth)) return;

        _degenTimer += (float)delta;
        float t = Mathf.Clamp(_degenTimer / DegenDuration, 0f, 1f);
        _displayedHealth = Mathf.Lerp(_degenStartHealth, _targetHealth, t);

        if (_fill != null && _health.MaxHealth > 0)
        {
            _fill.MaxValue = _health.MaxHealth;
            _fill.Value = _displayedHealth;
        }
    }

    public override void _ExitTree()
    {
        if (_health != null)
            _health.OnHealthChanged -= UpdateHealth;
    }
}