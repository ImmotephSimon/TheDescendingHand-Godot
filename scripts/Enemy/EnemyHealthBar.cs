using Godot;

[GlobalClass]
public partial class EnemyHealthBar : Node3D, IHealthBar
{
    private const float _visibleDuration = 5f;
    private const float _degenDisplayDuration = 0.2f;

    private Camera3D cam;
    private TextureProgressBar fill;
    private IHealth _health;

    private float _displayedHealth;
    private float _targetHealth;
    private float _degenStartHealth;
    private float _degenTimer;
    private double hideTime;

    public override void _Ready()
    {
        cam = GetViewport().GetCamera3D();
        fill = this.GetComponentInChildren<TextureProgressBar>(true);
        SetVisibility(false);
    }

    public void Bind(IHealth health)
    {
        _health = health;
        _health.OnHealthChanged += UpdateHealth;

        _displayedHealth = _health.CurrentHealth;
        _targetHealth = _health.CurrentHealth;
    }

    public void Unbind()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= UpdateHealth;
            _health = null;
        }
    }

    private void UpdateHealth(float current, float max, bool isInstant)
    {
        if (current >= max)
        {
            SetVisibility(false);
            return;
        }

        if (isInstant)
        {
            _displayedHealth = current;
            _targetHealth = current;
            if (fill != null && max > 0)
            {
                fill.MaxValue = max;
                fill.Value = current;
            }
        }
        else
        {
            _degenStartHealth = _displayedHealth;
            _targetHealth = current;
            _degenTimer = 0f;
        }

        SetVisibility(true);
        hideTime = Time.GetTicksMsec() / 1000.0 + _visibleDuration;
    }

    public override void _Process(double delta)
    {
        if (cam != null)
        {
            GlobalRotation = cam.GlobalRotation;
        }

        if (_displayedHealth != _targetHealth && _health != null)
        {
            _degenTimer += (float)delta;

            float t = Mathf.Clamp(_degenTimer / _degenDisplayDuration, 0f, 1f);
            _displayedHealth = Mathf.Lerp(_degenStartHealth, _targetHealth, t);

            if (fill != null && _health.MaxHealth > 0)
            {
                fill.MaxValue = _health.MaxHealth;
                fill.Value = _displayedHealth;
            }
        }

        if ((Time.GetTicksMsec() / 1000.0) >= hideTime)
            SetVisibility(false);
    }

    private void SetVisibility(bool isVisible)
    {
        Visible = isVisible;
    }

    public override void _ExitTree()
    {
        Unbind();
    }
}