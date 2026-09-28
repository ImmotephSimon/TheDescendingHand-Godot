using System;

public interface IStatContainer
{
    float GetStat(GameTag stat, TagContainer context, float baseValue = 0);
    float GetStat(GameTag stat);
    ModifierHandle AddModifier(StatModifier modifier, float duration = 0);
    void RemoveModifier(ModifierHandle handle, bool allowPrematureRemoval = false);
    void RemoveModifier(GameTag stat);
    void Listen(GameTag stat, Action<float> listener);
    void Listen(GameTag[] stats, Action<float> listener);
    void StopListening(GameTag stat, Action<float> listener);
    void StopListening(GameTag[] stats, Action<float> listener);

    (float added, float additive, float multiplicative) GetScaling(GameTag stat, TagContainer context);
    (float added, float additive, float multiplicative) GetScaling(GameTag stat);
}