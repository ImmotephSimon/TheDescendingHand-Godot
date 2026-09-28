[GlobalClass]
public partial class LevelAnchor : Node3D
{
    [Export] private Node3D anchor;

    public Node3D Anchor => anchor;

    [Export] public int LevelIndex;
}