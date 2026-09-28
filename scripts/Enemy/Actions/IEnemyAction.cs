public enum ActionState
{
    Ready,
    Running
}
public interface IEnemyAction
{
    float GetPriority();

    void StartAction();
    void UpdateAction(float delta);
    void StopAction();
    bool IsAvailable();
    bool CanBeInterrupted { get; }
    bool CanBeUpdated { get; }
}