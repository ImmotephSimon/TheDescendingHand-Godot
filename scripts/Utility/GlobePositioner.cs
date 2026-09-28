
[GlobalClass]
public partial class GlobePositioner : Node3D
{
    [Export] private PackedScene healthGlobePrefab;
    [Export] private PackedScene manaGlobePrefab;

    private readonly Vector2 healthScreenPosition = new(400, 930);
    private readonly Vector2 manaScreenPosition = new(1520, 930);

    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;

    private float distance = 1f;
    private Node3D _healthGlobe;
    private Node3D _manaGlobe;
    private Camera3D _targetCamera;
    private IEnumerable<LiquidWobble> liquids;

    public override void _Ready()
    {
        _targetCamera = GetViewport().GetCamera3D();

        if (healthGlobePrefab != null)
        {
            _healthGlobe = healthGlobePrefab.Instantiate<Node3D>();
            AddChild(_healthGlobe);
        }

        if (manaGlobePrefab != null)
        {
            _manaGlobe = manaGlobePrefab.Instantiate<Node3D>();
            AddChild(_manaGlobe);
        }

        liquids = this.GetComponentsInChildren<LiquidWobble>();
    }

    public void Initialize(PlayerStatsSync state)
    {
        foreach (var liquid in liquids)
        {
            liquid.Initialize(state);
        }
    }

    public override void _Process(double delta)
    {
        if (_targetCamera == null) return;

        Place(_healthGlobe, healthScreenPosition);
        Place(_manaGlobe, manaScreenPosition);
    }

    private void Place(Node3D globe, Vector2 screenPosition)
    {
        if (globe == null) return;

        Vector2 viewport = new(
            screenPosition.X / ReferenceWidth,
            screenPosition.Y / ReferenceHeight
        );

        Vector2 screenSize = GetViewport().GetVisibleRect().Size;
        Vector2 actualScreenPos = new(viewport.X * screenSize.X, viewport.Y * screenSize.Y);

        globe.GlobalPosition = _targetCamera.ProjectPosition(actualScreenPos, distance);
    }
}