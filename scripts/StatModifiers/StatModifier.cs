public enum MathOp
{
    Added,
    Additive,
    Multiplicative,
    Set
}



[GlobalClass]
public partial class StatModifier : Resource
{
    [Export] public GameTag Stat { get; set; }
    [Export] public MathOp Op { get; set; }
    [Export] public float Value { get; set; }
    [Export] public TagRequirement RequiredTags { get; set; }

    // Required by Godot for Resource instantiation
    public StatModifier() { }

    public StatModifier(GameTag stat, MathOp type, float value, TagRequirement requiredTags)
    {
        Stat = stat;
        Op = type;
        Value = value;
        RequiredTags = requiredTags;
    }

    public StatModifier(GameTag stat, MathOp type, float value)
        : this(stat, type, value, TagRequirement.Empty) { }

    public StatModifier(GameTag stat)
        : this(stat, MathOp.Set, 1f, TagRequirement.Empty) { }

    public override string ToString()
    {
        string statName = Stat != null ? Stat.ToString() : "Unknown";
        string reqStr = RequiredTags.IsElemental
            ? "Elemental"
            : !RequiredTags.IsEmpty
                ? RequiredTags.ToString()
                : "";

        string requirement = !string.IsNullOrEmpty(reqStr) ? $"{reqStr} " : "";

        if (Op == MathOp.Added)
        {
            int roundVal = Mathf.RoundToInt(Value);
            return $"{requirement}{statName}: {(roundVal >= 0 ? "+" : "-")}{Mathf.Abs(roundVal)}";
        }

        if (Op == MathOp.Additive)
        {
            int roundVal = Mathf.RoundToInt(Value * 100f);
            string prefix = roundVal >= 0 ? "Increased " : "Decreased ";
            return $"{prefix}{requirement}{statName}: {Mathf.Abs(roundVal)}%";
        }

        if (Op == MathOp.Multiplicative)
        {
            float percentChange = (Value - 1f) * 100f;
            int roundVal = Mathf.RoundToInt(percentChange);
            string prefix = roundVal >= 0 ? "More " : "Less ";
            return $"{prefix}{requirement}{statName}: {Mathf.Abs(roundVal)}%";
        }

        return $"{requirement}{statName}: {Mathf.RoundToInt(Value)}";
    }

    public string ToDescription()
    {
        string statName = Stat != null ? Stat.ToString() : "Unknown";
        string reqStr = RequiredTags.IsElemental
            ? "Elemental"
            : !RequiredTags.IsEmpty
                ? RequiredTags.ToString()
                : "";

        string requirement = !string.IsNullOrEmpty(reqStr) ? $"{reqStr} " : "";

        if (Op == MathOp.Added)
        {
            int roundVal = Mathf.RoundToInt(Value);
            return $"{requirement}{(roundVal >= 0 ? "+" : "-")}{Mathf.Abs(roundVal)} to {statName}";
        }

        if (Op == MathOp.Additive)
        {
            int roundVal = Mathf.RoundToInt(Value * 100f);
            string prefix = roundVal >= 0 ? "increased" : "decreased";
            return $"{Mathf.Abs(roundVal)}% {prefix} {requirement}{statName}";
        }

        if (Op == MathOp.Multiplicative)
        {
            int roundVal = Mathf.RoundToInt((Value - 1f) * 100f);
            string prefix = roundVal >= 0 ? "more" : "less";
            return $"{Mathf.Abs(roundVal)}% {prefix} {requirement}{statName}";
        }

        return $"{requirement}{statName}: {Mathf.RoundToInt(Value)}";
    }
}