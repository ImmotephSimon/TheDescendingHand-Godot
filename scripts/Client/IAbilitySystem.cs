using System;

public interface IAbilitySystem
{
    void RequestCancelCurrentCast();
    void RequestCancelAbility(int index);
    void RequestUseAbility(int  cardIndex);
}