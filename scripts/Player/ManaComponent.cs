[GlobalClass]
public partial class ManaComponent : Node3D, IMana
{
    private IStatContainer stats;
    private float _maxMana => stats.GetStat(GameTags.ModStatMana);
    private float _currentMana;

    public float CurrentMana => _currentMana;
    public float MaxMana => _maxMana;

    public event Action<float, float, bool> OnManaChanged;

    public override void _Ready()
    {
        stats = this.GetComponentInOwner<IStatContainer>();

        if (stats == null)
            GD.PushError($"'{Name}' has no IStatContainer");

        _currentMana = _maxMana;
        OnManaChanged?.Invoke(_currentMana, _maxMana, true);

        stats?.Listen(GameTags.ModStatMana, OnMaxManaChanged);
    }

    private void OnMaxManaChanged(float maxMana)
    {
        OnManaChanged?.Invoke(_currentMana, _maxMana, true);
    }

    public void Spend(float amount)
    {
        _currentMana = Godot.Mathf.Max(_currentMana - amount, 0f);
        OnManaChanged?.Invoke(_currentMana, _maxMana, true);
    }

    public void Restore(float amount, bool isInstant)
    {
        _currentMana = Godot.Mathf.Min(_currentMana + amount, _maxMana);
        OnManaChanged?.Invoke(_currentMana, _maxMana, isInstant);
    }
}