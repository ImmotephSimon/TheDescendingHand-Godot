[GlobalClass]
public partial class ManualConnect : Node3D
{
    [Export] private string _address = "127.0.0.1";
    [Export] private int _port = 7000;

    private string _status = "Press J to connect";

    public override void _Ready()
    {
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }

    public override void _ExitTree()
    {
        Multiplayer.ConnectedToServer -= OnConnectedToServer;
        Multiplayer.ConnectionFailed -= OnConnectionFailed;
        Multiplayer.ServerDisconnected -= OnServerDisconnected;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.J)
        {
            StartConnection();
        }
    }

    private void StartConnection()
    {
        var peer = new ENetMultiplayerPeer();
        Error err = peer.CreateClient(_address, _port);

        if (err == Error.Ok)
        {
            Multiplayer.MultiplayerPeer = peer;
            _status = "Connecting...";
        }
        else
        {
            _status = "Failed to trigger connect";
        }
    }

    private void OnConnectedToServer() => _status = "State: Connected";
    private void OnConnectionFailed() => _status = "State: ConnectionFailed";
    private void OnServerDisconnected() => _status = "State: ServerDisconnected";
}