[GlobalClass]
public partial class HealthComponent : Node3D, IHealth
{
    private float _currentHealth;
    private IEntity owner;
    private IStatContainer stats;

    public event Action<float, float, bool> OnHealthChanged;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    private float _maxHealth => stats.GetStat(GameTags.ModStatHealth);

    public override void _Ready()
    {
        owner = this.GetComponentInOwner<IEntity>();
        stats = this.GetComponentInOwner<IStatContainer>();

        if (stats == null)
            GD.PushError($"'{Name}' has no IStatContainer");

        stats?.Listen(GameTags.ModStatHealth, OnMaxHealthChanged);

        _currentHealth = _maxHealth;
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, true);
    }

    private void OnMaxHealthChanged(float maxHealth)
    {
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, true);
    }

    public void AdjustHealth(float healthAdjustment, object source, bool isInstant = true)
    {
        _currentHealth = Godot.Mathf.Clamp(
            _currentHealth + healthAdjustment,
            0,
            _maxHealth
        );
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, isInstant);

        GD.Print($"{owner} health now {_currentHealth} ({healthAdjustment:+#;-#;0} from {source ?? "Environment"})");

        if (_currentHealth <= 0)
            owner?.Die(source as IEntity);
    }

    public void Restore(float amount)
    {
        _currentHealth = Godot.Mathf.Min(
            _currentHealth + amount,
            MaxHealth);

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth, true);
    }
}