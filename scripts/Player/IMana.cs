using System;

public interface IMana
{
    float CurrentMana { get; }
    float MaxMana { get; }

    event Action<float, float, bool> OnManaChanged; // current, max, isInstant
}