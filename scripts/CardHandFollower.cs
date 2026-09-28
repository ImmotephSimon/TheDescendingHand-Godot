[GlobalClass]
public partial class CardHandFollower : Node
{
    [Export] private CardView _view;

    private Node3D _root;
    private Vector3 _targetPosition;
    private Quaternion _targetRotation = Quaternion.Identity;

    public void InitializePhysicalCard(CardDefinition definition, Node3D root, string keybindText)
    {
        _root = root;
        _view.InitializeTooltipCard(definition);
        _view.SetKeybindText(keybindText);
    }

    public void SetHandTransform(Vector3 position, Quaternion rotation)
    {
        _targetPosition = position;
        _targetRotation = rotation;
    }

    public void MoveTowardsHandTransform(float speed, float delta)
    {
        if (_root == null) return;
        float t = Mathf.Clamp(speed * delta, 0f, 1f);
        _root.Position = _root.Position.Lerp(_targetPosition, t);
        _root.Quaternion = _root.Quaternion.Slerp(_targetRotation, t);
    }

    public void UpdateParent(Node3D parent)
    {
        if (_root == null) return;
        _root.Reparent(parent, keepGlobalTransform: true);
        _root.Scale = Vector3.One * 0.2f;
    }

    public void DestroyCard() => _root?.QueueFree();
}