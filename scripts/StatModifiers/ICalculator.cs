using System.Collections.Generic;

public interface ICalculator
{
    Dictionary<GameTag, float> CalculateDamage(TagContainer tags, float effectiveness, GameTag damageConversion);
}