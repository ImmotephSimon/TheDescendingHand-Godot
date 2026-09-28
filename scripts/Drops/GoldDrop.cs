[GlobalClass]
public partial class GoldDrop : WorldDrop
{
    [Export] private int _amount;

    public void Initialize(int amount) => _amount = amount;

    public override void _Ready()
    {
        BodyEntered += body => {
            if (!Multiplayer.IsServer()) return;
            if (body.TryGetComponent<IPlayerCollection>(out var collection))
            {
                collection.AddGold(_amount);
                QueueFree();
            }
        };
    }
}