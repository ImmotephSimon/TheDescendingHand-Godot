[GlobalClass]
public partial class EnemyDefinition : Resource
{
    [Export] public EnemyType EnemyType { get; set; } = EnemyType.Normal;

    [ExportGroup("Visuals")]
    [Export] public PackedScene ModelPrefab { get; set; }

    [ExportGroup("Attacks")]
    [Export] public Godot.Collections.Array<EnemyAttackDefinition> Attacks { get; set; } = new();
}

public enum EnemyType { Normal, Magic, Rare, Unique }