[GlobalClass]
public partial class PassiveTreeComponent : Node
{
    [Export] private PassiveTreeData _data;
    private IStatContainer _stats;

    private readonly HashSet<string> _allocatedNodes = new();
    private readonly Dictionary<string, List<ModifierHandle>> _nodeModifiers = new();
    private readonly Dictionary<GameTag, int> _allocatedStats = new();

    public IReadOnlyCollection<string> AllocatedNodes => _allocatedNodes;
    public event Action<string, bool> OnNodeStateChanged;

    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            _stats = this.GetComponentInOwner<IStatContainer>();
            Debug.Assert(_stats != null, "Invalid stats container.");
        }
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public void AllocateNodeServerRpc(string nodeName)
    {
        if (Multiplayer.IsServer() && TryAllocateNode(nodeName))
            Rpc(nameof(ClientUpdateNodeState), nodeName, true);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = false)]
    public void RefundNodeServerRpc(string nodeName)
    {
        if (Multiplayer.IsServer() && TryRefundNode(nodeName))
            Rpc(nameof(ClientUpdateNodeState), nodeName, false);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void ClientUpdateNodeState(string nodeName, bool allocated)
    {
        if (allocated) _allocatedNodes.Add(nodeName);
        else _allocatedNodes.Remove(nodeName);
        OnNodeStateChanged?.Invoke(nodeName, allocated);
    }

    private bool TryAllocateNode(string nodeName)
    {
        if (_allocatedNodes.Contains(nodeName)) return false;
        PassiveNode node = FindNode(nodeName);
        if (node == null || !CanAllocate(node) || !HasSufficientStats(node)) return false;

        DeductStats(node);
        _allocatedNodes.Add(node.Name);
        ApplyModifiers(node);
        return true;
    }

    private bool TryRefundNode(string nodeName)
    {
        if (!_allocatedNodes.Contains(nodeName)) return false;
        PassiveNode node = FindNode(nodeName);
        if (node == null || node.Role == PassiveNodeRole.Starter) return false;

        _allocatedNodes.Remove(node.Name);
        RemoveModifiers(node);
        RestoreStats(node);
        return true;
    }

    private bool HasSufficientStats(PassiveNode node)
    {
        foreach (PassiveNodeCost req in node.Requirement)
            if (_stats.GetStat(req.Attribute) < req.Amount) return false;

        foreach (PassiveNodeCost cost in node.Cost)
        {
            int allocated = GetAllocatedStat(cost.Attribute);
            if (_stats.GetStat(cost.Attribute) - allocated < cost.Amount) return false;
        }
        return true;
    }

    private void DeductStats(PassiveNode node)
    {
        foreach (PassiveNodeCost cost in node.Cost)
            _allocatedStats[cost.Attribute] = GetAllocatedStat(cost.Attribute) + cost.Amount;
    }

    private void RestoreStats(PassiveNode node)
    {
        foreach (PassiveNodeCost cost in node.Cost)
            _allocatedStats[cost.Attribute] = GetAllocatedStat(cost.Attribute) - cost.Amount;
    }

    private int GetAllocatedStat(GameTag attribute) =>
        _allocatedStats.TryGetValue(attribute, out int val) ? val : 0;

    private bool CanAllocate(PassiveNode node)
    {
        if (node.Role == PassiveNodeRole.Starter) return true;
        foreach (PassiveNodeConnection connection in _data.Connections)
        {
            if ((connection.A == node.Name && _allocatedNodes.Contains(connection.B)) ||
                (connection.B == node.Name && _allocatedNodes.Contains(connection.A)))
                return true;
        }
        return false;
    }

    private void ApplyModifiers(PassiveNode node)
    {
        PassiveNodeType type = FindType(node.Type);
        if (type == null) return;

        List<ModifierHandle> handles = new();
        foreach (StatModifier passiveModifier in type.Modifiers)
            handles.Add(_stats.AddModifier(passiveModifier));

        _nodeModifiers[node.Name] = handles;
    }

    private void RemoveModifiers(PassiveNode node)
    {
        if (!_nodeModifiers.Remove(node.Name, out List<ModifierHandle> handles)) return;
        foreach (ModifierHandle handle in handles)
            _stats.RemoveModifier(handle);
    }

    private PassiveNode FindNode(string nodeName)
    {
        foreach (PassiveNode node in _data.Nodes)
            if (node.Name == nodeName) return node;
        return null;
    }

    private PassiveNodeType FindType(string typeName)
    {
        foreach (PassiveNodeType type in _data.NodeTypes)
            if (type.Name == typeName) return type;
        return null;
    }
}