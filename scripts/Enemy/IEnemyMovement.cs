using Godot;

public interface IEnemyMovement
{
    bool HasReachedDestination();
    bool IsWithinStoppingDistance(Vector3 position);
    void MoveTo(Vector3 position);
    void StopMovement();
    void RotateTowardsTarget(Node3D target);
    float GetFacingAngle(Node3D target);
    void LockRotation(Node3D target);
    void UnlockRotation();
}