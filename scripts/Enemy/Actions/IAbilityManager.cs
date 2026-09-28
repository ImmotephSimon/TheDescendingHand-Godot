public interface IAbilityManager
{
    bool IsReady(IEnemyAttack attack);
    void StartCooldown(IEnemyAttack attack);
}