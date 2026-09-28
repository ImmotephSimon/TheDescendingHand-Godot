using Godot;
using System.Threading.Tasks;

[GlobalClass]
public partial class DungeonManager : Node
{
    [Export] private PackedScene dungeonGeneratorPrefab;
    [Export] private DirectionalLight3D directionalLight;
    [Export] private Node3D overworld;
    [Export] private PackedScene stairsPrefab;

    private readonly Dictionary<Node3D, DungeonGenerator> dungeonCache = new();
    private DungeonGenerator activeDungeon;
    private bool isGenerating;

    public int ZoneLevel => activeDungeon != null ? activeDungeon.ZoneLevel : 0;
    public static DungeonManager Instance { get; private set; }

    private DropsComponent _drops;

    public DungeonGenerator ActiveDungeon => IsOverworld ? null : dungeonStack.Peek();
    public bool IsOverworld => dungeonStack.Count == 0;

    public Action<int> OnZoneLevelChanged { get; internal set; }

    private Stack<DungeonGenerator> dungeonStack = new();

    private Node3D stairs;

    public override void _EnterTree()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;
        _drops = this.GetComponentInChildren<DropsComponent>();

        if (overworld != null)
        {
            var stairsNode = overworld.GetComponentInChildren<Stairs>();
            if (stairsNode != null)
            {
                stairs = stairsNode;
                stairs.GetParent()?.RemoveChild(stairs);
                AddChild(stairs);
            }
        }
    }

    public void EnterDungeon(Node3D anchor)
    {
        if (isGenerating || anchor == null) return;

        SetCurrentVisible(false);
        if (overworld != null) overworld.Visible = false;

        if (dungeonCache.TryGetValue(anchor, out var cachedDungeon))
        {
            dungeonStack.Push(cachedDungeon);
            AssignDungeon(cachedDungeon);
            return;
        }

        Vector3 spawnPosition = new Vector3(
            Mathf.Round(anchor.GlobalPosition.X / 3f) * 3f,
            anchor.GlobalPosition.Y + 3f,
            Mathf.Round(anchor.GlobalPosition.Z / 3f) * 3f
        );

        if (dungeonGeneratorPrefab == null) return;

        var newDungeon = dungeonGeneratorPrefab.Instantiate<DungeonGenerator>();
        AddChild(newDungeon);
        newDungeon.GlobalPosition = spawnPosition;
        newDungeon.GlobalRotation = anchor.GlobalRotation;

        newDungeon.ZoneLevel = (activeDungeon != null) ? activeDungeon.ZoneLevel + 1 : 1;

        dungeonCache[anchor] = newDungeon;
        dungeonStack.Push(newDungeon);

        AssignDungeon(newDungeon);
        _ = GenerateRoutine(newDungeon);
    }

    public void LeaveDungeon()
    {
        if (isGenerating || dungeonStack.Count == 0) return;

        SetCurrentVisible(false);
        dungeonStack.Pop();

        if (IsOverworld)
        {
            activeDungeon = null;
            if (overworld != null) overworld.Visible = true;
            if (directionalLight != null) directionalLight.Visible = true;
            OnZoneLevelChanged?.Invoke(0);
        }
        else
        {
            AssignDungeon(dungeonStack.Peek());
        }
    }

    private void AssignDungeon(DungeonGenerator dungeon)
    {
        activeDungeon = dungeon;
        if (directionalLight != null) directionalLight.Visible = IsOverworld;
        SetCurrentVisible(true);

        OnZoneLevelChanged?.Invoke(ZoneLevel);
    }

    private void SetCurrentVisible(bool visible)
    {
        if (activeDungeon != null)
        {
            activeDungeon.Visible = visible;
        }
    }

    private async Task GenerateRoutine(DungeonGenerator dungeon)
    {
        isGenerating = true;
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        dungeon.StartGenerating();
        isGenerating = false;
    }

    internal void OnDungeonCompleted(Node3D stairsAnchor)
    {
        if (stairsPrefab == null || stairsAnchor == null) return;

        var newStairs = stairsPrefab.Instantiate<Node3D>();
        AddChild(newStairs);
        newStairs.GlobalPosition = stairsAnchor.GlobalPosition;
        newStairs.GlobalRotation = stairsAnchor.GlobalRotation;
    }

    internal void RegisterDungeonChest(HingeController hingeController)
    {
        if (hingeController == null) return;

        Action handler = null;
        handler = () =>
        {
            hingeController.PlayerInteracted -= handler;
            for (int i = 0; i < 5; i++)
            {
                _drops?.DropFromChest(
                    hingeController.GlobalPosition,
                    -hingeController.GlobalTransform.Basis.Z
                );
            }
        };

        hingeController.PlayerInteracted += handler;
    }
}