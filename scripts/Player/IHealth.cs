using System;

public interface IHealth
{
    float CurrentHealth { get; }
    float MaxHealth { get; }

    event Action<float, float, bool> OnHealthChanged; // current, max, isInstant

    void AdjustHealth(float finalDamage, object source, bool isInstant = true);
}