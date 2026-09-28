using System;
using System.Collections.Generic;


public class ItemDropInstance : IInventoryItem
{
    public Guid Instance { get; protected set; } = Guid.NewGuid();
    public Vector2I Size => BaseType.InventorySize;
    public Texture2D Icon => BaseType.Appearance.Icon;
    public ItemDefinition BaseType { get; }
    public Rarity Rarity { get; }
    public List<AffixInstance> Explicits { get; }
    public Dictionary<AffixInstance, List<ModifierHandle>> ActiveExplicits { get; } = new();
    public List<ModifierHandle> ActiveImplicits { get; } = new();

    public List<ItemUseComponent> Components { get; } = new();

    public Guid InventoryId => BaseType.Id;

    public ItemDropInstance(ItemDefinition baseType, Rarity rarity, List<AffixInstance> explicits)
    {
        BaseType = baseType;
        Rarity = rarity;
        Explicits = explicits ?? new List<AffixInstance>();

        foreach (var def in BaseType.Components)
        {
            Components.Add(def.CreateRuntimeComponent());
        }
    }

    public void ApplyModifiers(IEntity owner)
    {
        foreach (var affix in BaseType.Implicits)
        {
            var handle = owner.Stats.AddModifier(affix.Modifier);
            ActiveImplicits.Add(handle);
        }

        foreach (var affix in Explicits)
        {
            var handles = new List<ModifierHandle>();
            foreach (var mod in affix.ToStatModifiers(BaseType.Implicits))
                handles.Add(owner.Stats.AddModifier(mod));

            ActiveExplicits.Add(affix, handles);
        }
    }

    public void ClearModifiers(IEntity owner)
    {
        foreach (var handle in ActiveImplicits)
            owner.Stats.RemoveModifier(handle);
        ActiveImplicits.Clear();

        foreach (var handles in ActiveExplicits.Values)
            foreach (var handle in handles)
                owner.Stats.RemoveModifier(handle);

        ActiveExplicits.Clear();
    }

    public void Display()
    {
        throw new NotImplementedException();
    }
}