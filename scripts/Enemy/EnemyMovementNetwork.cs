using System;
using Godot;

[GlobalClass]
public partial class EnemyMovementNetwork : Node3D
{
    [Export] private NavigationAgent3D agent;
    [Export] private Node3D brain;
    [Export] private float speedSmoothing = 10f;

    private float clientSmoothSpeed;
    private IAnimationHandler animationController;
    private IEnemyMovement movementController;
    private float networkSpeed;
    private Node animationObject;

    public float CurrentSpeed => networkSpeed;

    public override void _Ready()
    {
        movementController = this.GetComponentInChildren<IEnemyMovement>();
        if (!Multiplayer.IsServer())
        {
            SetProcess(true);
        }
    }

    public override void _EnterTree()
    {
        if (Multiplayer.IsServer())
        {
            if (movementController is Node movNode) movNode.SetProcess(true);
            if (brain != null) brain.SetProcess(true);
            SetProcess(true);
        }
    }

    public override void _Process(double delta)
    {
        if (Multiplayer.IsServer())
        {
            UpdateServerMovementState((float)delta);
        }
        else
        {
            UpdateClientVisuals((float)delta);
        }
    }

    private void UpdateServerMovementState(float delta)
    {
        float speed = 0f;
        if (agent != null)
        {
            Vector3 velocity = (agent.GetNextPathPosition() - GlobalPosition) / delta;
            speed = velocity.Length();
        }
        networkSpeed = speed;
        Rpc(nameof(SyncSpeed), networkSpeed);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false)]
    private void SyncSpeed(float speed)
    {
        networkSpeed = speed;
    }

    private void UpdateClientVisuals(float delta)
    {
        if (animationObject == null)
        {
            animationController = this.GetComponentInChildren<IAnimationHandler>();
            if (animationController == null)
                return;

            animationObject = (Node)animationController;
        }

        clientSmoothSpeed = Mathf.Lerp(clientSmoothSpeed, CurrentSpeed, delta * speedSmoothing);

        if (Mathf.IsZeroApprox(CurrentSpeed) && clientSmoothSpeed < 0.05f)
            clientSmoothSpeed = 0f;

        animationController.SetSpeed(clientSmoothSpeed);
    }
}