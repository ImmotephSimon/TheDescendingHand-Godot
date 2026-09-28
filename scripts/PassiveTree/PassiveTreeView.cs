using System.Text;
[GlobalClass]
public partial class PassiveTreeView : Control
{
    [Export] private PassiveTreeData _data;
    [Export] private Control _container;
    [Export] private PackedScene _nodePrefab;
    [Export] private PackedScene _connectionPrefab;
    [Export] private Control _window;
    [Export] private PackedScene _tooltipPrefab;
    [Export] private Control _tooltipLayer;

    [Export] private float _minZoom = 0.5f;
    [Export] private float _maxZoom = 2f;
    [Export] private float _zoomSpeed = 0.1f;

    private readonly Dictionary<string, PassiveNode> _nodes = new();
    private readonly Dictionary<string, PassiveNodeType> _nodeTypes = new();
    private readonly Dictionary<string, PassiveNodeView> _nodeViews = new();

    private PassiveTreeComponent _targetComponent;
    private PassiveNodeView _selectedNodeView;
    private PassiveTooltipView _tooltipInstance;
    private bool _isDragging;

    public PassiveTreeData Data => _data;
    public PassiveNode SelectedNode => _selectedNodeView?.NodeData;
    public bool IsTreeVisible => _window.Visible;

    public override void _EnterTree()
    {
        _window.VisibilityChanged += () =>
        {
            if (_window.Visible)
                RefreshAllNodeStates();
        };
    }

    public override void _Ready()
    {
        ClientBridge.Instance.OnClientPlayerReady += HandlePlayerReady;

        _tooltipInstance = _tooltipPrefab.Instantiate<PassiveTooltipView>();
        _tooltipLayer.AddChild(_tooltipInstance);
        _tooltipInstance.HideTooltip();

        Build();
    }

    public override void _ExitTree()
    {
        if (ClientBridge.Instance != null)
            ClientBridge.Instance.OnClientPlayerReady -= HandlePlayerReady;
        if (_targetComponent != null)
            _targetComponent.OnNodeStateChanged -= HandleNodeStateChanged;
    }

    public void Initialize(PassiveTreeComponent targetComponent)
    {
        if (_targetComponent != null)
            _targetComponent.OnNodeStateChanged -= HandleNodeStateChanged;

        _targetComponent = targetComponent;

        if (_targetComponent != null)
        {
            _targetComponent.OnNodeStateChanged += HandleNodeStateChanged;
            RefreshAllNodeStates();
        }
        SetVisible(false);
    }

    private void HandlePlayerReady(ClientPlayer player)
    {
        if (!player.TryGetComponent(out PassiveTreeComponent passiveTree))
        {
            GD.PrintErr("[PassiveTreeView] No passive tree component.");
            return;
        }
        Initialize(passiveTree);
    }

    public void Build()
    {
        _nodeTypes.Clear();
        _nodes.Clear();
        _nodeViews.Clear();

        foreach (PassiveNodeType type in _data.NodeTypes)
            _nodeTypes[type.Name] = type;

        foreach (PassiveNode node in _data.Nodes)
        {
            _nodes[node.Name] = node;
            PassiveNodeView instance = _nodePrefab.Instantiate<PassiveNodeView>();
            _container.AddChild(instance);
            instance.SetNode(node, _nodeTypes[node.Type], OnNodeClicked);

            instance.HoverEnter += nodeView => _tooltipInstance.Show(
                BuildTooltipData(nodeView.NodeData, nodeView.NodeType),
                nodeView);

            instance.HoverExit += _ => _tooltipInstance.HideTooltip();

            _nodeViews[node.Name] = instance;
        }

        foreach (PassiveNodeConnection connection in _data.Connections)
            CreateConnection(connection);
    }

    public void RequestAllocateSelected()
    {
        if (_targetComponent == null || _selectedNodeView == null) return;
        _targetComponent.AllocateNodeServerRpc(_selectedNodeView.NodeData.Name);
    }

    public void RequestRefundSelected()
    {
        if (_targetComponent == null || _selectedNodeView == null) return;
        _targetComponent.RefundNodeServerRpc(_selectedNodeView.NodeData.Name);
    }

    private void HandleNodeStateChanged(string nodeName, bool isAllocated)
    {
        if (_nodeViews.TryGetValue(nodeName, out PassiveNodeView view))
        {
            view.SetAllocated(isAllocated);
        }
    }

    private void RefreshAllNodeStates()
    {
        if (_targetComponent == null) return;

        foreach (var kvp in _nodeViews)
        {
            bool isAllocated = _targetComponent.AllocatedNodes.Contains(kvp.Key);
            kvp.Value.SetAllocated(isAllocated);
        }
    }

    private void OnNodeClicked(PassiveNodeView nodeView, MouseButton button)
    {
        if (_selectedNodeView != null)
            _selectedNodeView.SetHighlight(false);

        _selectedNodeView = nodeView;
        _selectedNodeView.SetHighlight(true);

        RequestAllocateSelected();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Left)
            {
                _isDragging = mb.Pressed;
            }
            else if (mb.Pressed)
            {
                float zoomDir = mb.ButtonIndex == MouseButton.WheelUp ? 1f : (mb.ButtonIndex == MouseButton.WheelDown ? -1f : 0f);
                if (zoomDir != 0f)
                {
                    float currentScale = _container.Scale.X;
                    float zoom = Mathf.Clamp(currentScale + zoomDir * _zoomSpeed, _minZoom, _maxZoom);
                    _container.Scale = Vector2.One * zoom;
                }
            }
        }
        else if (_isDragging && @event is InputEventMouseMotion mm)
        {
            Vector2 position = _container.Position + mm.Relative;
            _container.Position = ClampPosition(position);
        }
    }

    private Vector2 ClampPosition(Vector2 targetPosition)
    {
        float currentScale = _container.Scale.X;
        Vector2 containerSize = _container.Size * currentScale;
        Vector2 windowSize = _window.Size;

        float maxX = Mathf.Max(0f, (containerSize.X - windowSize.X) * 0.5f);
        float maxY = Mathf.Max(0f, (containerSize.Y - windowSize.Y) * 0.5f);

        return new Vector2(
            Mathf.Clamp(targetPosition.X, -maxX, maxX),
            Mathf.Clamp(targetPosition.Y, -maxY, maxY));
    }

    private void CreateConnection(PassiveNodeConnection connection)
    {
        PassiveNode a = _nodes[connection.A];
        PassiveNode b = _nodes[connection.B];

        Control line = _connectionPrefab.Instantiate<Control>();
        _container.AddChild(line);

        Vector2 start = a.Position;
        Vector2 end = b.Position;
        Vector2 direction = end - start;

        line.Size = new Vector2(direction.Length(), line.Size.Y);
        line.PivotOffset = line.Size * 0.5f;
        line.Position = (start + end) * 0.5f - line.Size * 0.5f;
        line.Rotation = Mathf.Atan2(direction.Y, direction.X);
    }


    private PassiveTooltipData BuildTooltipData(PassiveNode node, PassiveNodeType type)
    {
        string description;

        if (!string.IsNullOrEmpty(type.DescriptionOverride))
        {
            description = type.DescriptionOverride;
        }
        else
        {
            var modifiers = new StringBuilder();
            foreach (StatModifier mod in type.Modifiers)
                modifiers.AppendLine(mod.ToDescription());

            description = modifiers.ToString();
        }

        var cost = new StringBuilder();
        if (node.Cost.Count > 0)
        {
            cost.AppendLine("Cost:");
            foreach (var c in node.Cost)
                cost.AppendLine($"  {c.Amount} {c.Attribute}");
        }

        var requirement = new StringBuilder();
        if (node.Requirement.Count > 0)
        {
            requirement.AppendLine("Requires:");
            foreach (var r in node.Requirement)
                requirement.AppendLine($"  {r.Amount} {r.Attribute}");
        }

        return new PassiveTooltipData
        {
            Title = node.Role == PassiveNodeRole.Keystone
                ? node.Name
                : string.Join(" & ", type.Modifiers.Select(x => x.Stat)),

            RoleText = node.Role == PassiveNodeRole.Keystone
                ? "Keystone"
                : "",

            DescriptionText = description,
            CostText = cost.ToString().TrimEnd(),
            RequirementText = requirement.ToString().TrimEnd(),
        };
    }
}