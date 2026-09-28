using System;
using System.Collections.Generic;


public class AffixInstance
{
    public AffixDefinition Definition;
    public float Tier = 1f;
    private List<StatModifier> CachedModifiers;


    private float ScaleValue(float baseValue, MathOp op)
    {
        if (baseValue == 0f) return 0f;

        float scaled = baseValue * Tier;

        if (op == MathOp.Added)
        {
            int sign = Math.Sign(baseValue);
            return sign * Mathf.Max(1, Mathf.RoundToInt(Mathf.Abs(scaled)));
        }

        if (op == MathOp.Multiplicative)
        {
            if (baseValue > 1f) return Mathf.Max(1.01f, scaled);
            if (baseValue < 1f) return Mathf.Min(0.99f, scaled);
            return scaled;
        }

        float minMagnitude = Mathf.Abs(baseValue) >= 1f ? 1f : 0.01f;
        return Math.Sign(baseValue) * Mathf.Max(minMagnitude, Mathf.Abs(scaled));
    }

    public List<StatModifier> ToStatModifiers(List<ItemImplicit> itemImplicits)
    {
        if (CachedModifiers != null)
            return CachedModifiers;

        CachedModifiers = new List<StatModifier>();

        for (int i = 0; i < Definition.Mods.Count; i++)
        {
            var mod = Definition.Mods[i];
            float scaledValue = ScaleValue(mod.Value, mod.Op);

            if (mod.Stat == GameTags.ModImplicit)
            {
                foreach (var implicitMod in itemImplicits)
                {
                    if (!implicitMod.IsScalable) continue;

                    CachedModifiers.Add(new StatModifier(implicitMod.Modifier.Stat, mod.Op, scaledValue, mod.RequiredTags));
                }
            }
            else
            {
                CachedModifiers.Add(new StatModifier(mod.Stat, mod.Op, scaledValue, mod.RequiredTags));
            }
        }

        return CachedModifiers;
    }

    public string GetDisplayText(List<ItemImplicit> itemImplicits)
    {
        if (!string.IsNullOrWhiteSpace(Definition.NameOverride))
            return $"{Definition.NameOverride}: {string.Join(", ", ToStatModifiers(itemImplicits).ConvertAll(m => m.Value.ToString()))}";

        return string.Join(", ", ToStatModifiers(itemImplicits).ConvertAll(m => m.ToString()));
    }
}