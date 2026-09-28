[GlobalClass]
public partial class LevelComponent : Node
{
    [Export] private BalanceCurves balanceCurves;
    [Export] private int level = 1;

    public int Level => level;
    public float Progress => (float)_experience / _levelExperience;

    private IStatContainer stats;
    private IEntity owner;
    private ModifierHandle _modifierHandle;

    private int _experience;
    private int _levelExperience = 20;

    public event Action<int, float> OnExperienceChanged;

    public override void _Ready()
    {
        stats = this.GetComponentInParent<IStatContainer>();
        owner = this.GetComponentInParent<IEntity>();

        if (GameWorld.Instance != null)
            GameWorld.Instance.EntityDied += OnEntityDied;

        ApplyLevelStats();
    }

    public override void _ExitTree()
    {
        if (GameWorld.Instance != null)
            GameWorld.Instance.EntityDied -= OnEntityDied;
    }

    private void OnEntityDied(IEntity victim, IEntity killer)
    {
        if (killer == owner && victim is IExperienceSource source)
        {
            AddExperience(source.ExperienceReward);
        }
    }

    private void AddExperience(int amount)
    {
        _experience += amount;

        while (_experience >= _levelExperience)
        {
            _experience -= _levelExperience;
            level++;

            UpdateLevelExperienceRequirement();
            ApplyLevelStats();
        }

        OnExperienceChanged?.Invoke(level, Progress);
    }

    private void UpdateLevelExperienceRequirement()
    {
        _levelExperience = Mathf.RoundToInt(_levelExperience * 1.1f);
    }

    public void LevelUp()
    {
        level++;
        _experience = 0;

        ApplyLevelStats();
        OnExperienceChanged?.Invoke(level, Progress);
    }

    private void ApplyLevelStats()
    {
        if (stats == null || balanceCurves == null) return;

        if (_modifierHandle.IsValid)
            stats.RemoveModifier(_modifierHandle);

        _modifierHandle = stats.AddModifier(
            new StatModifier(
                GameTags.ModStatHealth,
                MathOp.Added,
                balanceCurves.ExpectedPlayerLife.Sample(level)
            ));
    }
}