public class PatrolAction : IEnemyAction
{
    private enum PatrolState { Moving, Waiting }

    private float idleDuration = 3f;
    private readonly IEnemyMovement movementHandler;
    private readonly Vector3 anchorPosition;
    private readonly float radius;
    private float waitTimer;
    private PatrolState patrolState;

    public bool CanBeInterrupted => true;
    public bool CanBeUpdated => true;

    public PatrolAction(Node3D owner, float radius)
    {
        movementHandler = owner.GetComponentInOwner<IEnemyMovement>();
        anchorPosition = owner.GlobalPosition;
        this.radius = radius;
    }

    public bool IsAvailable() => true;
    public float GetPriority() => 10f;

    public void StartAction() => MoveToNextPoint();

    public void UpdateAction(float delta)
    {
        if (patrolState == PatrolState.Waiting)
        {
            waitTimer -= delta;
            if (waitTimer <= 0f) MoveToNextPoint();
            return;
        }

        if (movementHandler != null && movementHandler.HasReachedDestination())
        {
            movementHandler.StopMovement();
            patrolState = PatrolState.Waiting;
            waitTimer = idleDuration;
        }
    }

    public void StopAction() => movementHandler?.StopMovement();

    private void MoveToNextPoint()
    {
        if (movementHandler == null) return;

        patrolState = PatrolState.Moving;
        var randDir = new Vector3((float)GD.RandRange(-1, 1), 0, (float)GD.RandRange(-1, 1)).Normalized();
        Vector3 randomPoint = anchorPosition + randDir * (float)GD.RandRange(0, radius);
        movementHandler.MoveTo(randomPoint);
    }

    public void UpdateAction()
    {
        throw new NotImplementedException();
    }
}