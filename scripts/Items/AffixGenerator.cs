using System;
using System.Collections.Generic;
using System.Linq;


public static class AffixGenerator
{
    public static List<AffixInstance> Generate(ItemDefinition item, Rarity rarity, System.Random seed, int misfortune = 1)
    {
        var results = new List<AffixInstance>();

        int targetCount = RollAffixCountForRarity(rarity, seed);
        GD.Print($"Rarity: {rarity.ResourceName} | Min: {rarity.MinAffixes} | Max: {rarity.MaxAffixes} | TargetCount: {targetCount}");
        var selectedDefinitions = RollWeightedAffixDefinitions(item, targetCount, seed);

        foreach (var def in selectedDefinitions)
        {
            var instance = CreateAffixInstanceWithRolledValue(def, seed, misfortune);

            if (RollsPositiveForMutationChance(seed))
            {
                instance = ApplyMutationOrConversion(instance, seed);
            }

            results.Add(instance);
        }
        GD.Print($"Generated affixes: {string.Join(", ", results.Select(x => $"{x.GetDisplayText(item.Implicits)})"))}");
        return results;
    }

    public static List<AffixDefinition> RollWeightedAffixDefinitions(
    ItemDefinition item,
    int count,
    System.Random rng)
    {
        var results = new List<AffixDefinition>();

        var equipComp = item.Components.OfType<EquipComponentDefinition>().FirstOrDefault();
        if (equipComp == null || equipComp.EquipmentType == null)
            return results;

        var pool = equipComp.EquipmentType.ModifierPool;
        if (pool == null || pool.Entries == null || pool.Entries.Count == 0)
            return results;

        SplitAffixPool(pool, out List<ModifierPoolEntry> available, out List<ModifierPoolEntry> locked);

        var tagPool = new HashSet<GameTag>();

        for (int i = 0; i < count && available.Count > 0; i++)
        {
            float totalWeight = 0f;
            foreach (var entry in available)
                totalWeight += entry.Weight;

            if (totalWeight <= 0f) break;

            double roll = rng.NextDouble() * totalWeight;
            float currentWeight = 0f;

            for (int j = 0; j < available.Count; j++)
            {
                currentWeight += available[j].Weight;
                if (roll < currentWeight)
                {
                    var chosen = available[j].Definition;
                    if (chosen != null)
                    {
                        results.Add(chosen);

                        if (chosen.Tags != null)
                            foreach (var tag in chosen.Tags)
                                tagPool.Add(tag);
                    }

                    available.RemoveAt(j);
                    break;
                }
            }

            UpdateLockedAffixes(available, locked, tagPool);
        }

        return results;
    }

    private static void SplitAffixPool(ModifierPool pool, out List<ModifierPoolEntry> available, out List<ModifierPoolEntry> locked)
    {
        available = new List<ModifierPoolEntry>();
        locked = new List<ModifierPoolEntry>();
        foreach (var entry in pool.Entries)
        {
            if (entry.Definition != null && entry.Definition.Prerequisites != null && entry.Definition.Prerequisites.Count > 0)
                locked.Add(entry);
            else
                available.Add(entry);
        }
    }

    private static void UpdateLockedAffixes(List<ModifierPoolEntry> pool, List<ModifierPoolEntry> locked, HashSet<GameTag> tagPool)
    {
        for (int k = locked.Count - 1; k >= 0; k--)
        {
            if (locked[k].Definition.Prerequisites.All(t => tagPool.Contains(t)))
            {
                pool.Add(locked[k]);
                locked.RemoveAt(k);
            }
        }
    }

    private static AffixInstance CreateAffixInstanceWithRolledValue(
        AffixDefinition def,
        System.Random rng,
        int misfortune)
    {
        float effectiveRolls = 1f + misfortune;
        float u = (float)rng.NextDouble();
        float tier = 1f - Mathf.Pow(1f - u, 1f / effectiveRolls);

        return new AffixInstance
        {
            Definition = def,
            Tier = tier
        };
    }


    private static bool RollsPositiveForMutationChance(System.Random rng)
    {
        return rng.NextDouble() < 0.05;
    }

    private static AffixInstance ApplyMutationOrConversion(AffixInstance original, System.Random rng)
    {
        return original;
    }
    private static int RollAffixCountForRarity(Rarity rarity, System.Random rng)
    {
        if (rarity == null) return 0;
        return rng.Next(rarity.MinAffixes, rarity.MaxAffixes + 1);
    }

}