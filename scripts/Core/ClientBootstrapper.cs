[GlobalClass]
public partial class ClientBootstrapper : Node
{
    private const int DefaultPort = 7777;
    private const string DefaultAddress = "127.0.0.1";

    public override void _Ready()
    {
        //var prefab = GD.Load<PackedScene>("res://scenes/client_bridge.tscn");
        //if (prefab != null)
        //{
        //    var bridge = prefab.Instantiate();
        //    GetTree().Root.CallDeferred(Node.MethodName.AddChild, bridge);
        //}

        string[] args = OS.GetCmdlineArgs();
        NetworkMode mode = GetNetworkMode(args);

        GD.Print($"[NetworkBootstrapper] Starting as {mode}");

        switch (mode)
        {
            case NetworkMode.Client:
                StartClient(DefaultAddress, DefaultPort);
                break;

            case NetworkMode.Server:
            case NetworkMode.Host:
                StartServer(DefaultPort);
                break;
        }

        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ServerDisconnected += OnServerDisconnected;
    }

    private void StartServer(int port)
    {
        var peer = new ENetMultiplayerPeer();
        Error error = peer.CreateServer(port);
        if (error != Error.Ok)
        {
            GD.PushError($"[NetworkBootstrapper] Failed to start server: {error}");
            return;
        }

        Multiplayer.MultiplayerPeer = peer;
    }

    private void StartClient(string address, int port)
    {
        var peer = new ENetMultiplayerPeer();
        Error error = peer.CreateClient(address, port);
        if (error != Error.Ok)
        {
            GD.PushError($"[NetworkBootstrapper] Failed to start client: {error}");
            return;
        }

        Multiplayer.MultiplayerPeer = peer;
    }

    private NetworkMode GetNetworkMode(string[] args)
    {
        foreach (var arg in args)
        {
            switch (arg.ToLower())
            {
                case "-server":
                    return NetworkMode.Server;

                case "-host":
                    return NetworkMode.Host;

                case "-client":
                    return NetworkMode.Client;
            }
        }

        if (OS.HasFeature("editor"))
        {
            return NetworkMode.Host;
        }

        GD.PushError("[NetworkBootstrapper] No network mode specified. Use -client, -server, or -host.");
        GetTree().Quit();
        return NetworkMode.Client;
    }

    private void OnConnectedToServer()
    {
        GD.Print("[NetworkBootstrapper] Client connected to server.");
    }

    private void OnServerDisconnected()
    {
        GD.Print("[NetworkBootstrapper] Client disconnected from server.");
    }

    private enum NetworkMode
    {
        Client,
        Server,
        Host
    }
}