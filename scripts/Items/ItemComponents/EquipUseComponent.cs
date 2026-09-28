[GlobalClass]
public partial class EquipUseComponent : ItemUseComponent, IUsable
{
    public EquipComponentDefinition Definition { get; }

    public EquipUseComponent(EquipComponentDefinition definition)
    {
        Definition = definition;
    }

    public void Equip(ItemDropInstance instance, IEntity user)
    {
        instance.ApplyModifiers(user);
        ClientBridge.Instance.EquipmentVisuals.SetEquipment(Definition.EquipmentType, instance.BaseType);
    }

    public void Unequip(ItemDropInstance instance, IEntity user)
    {
        instance.ClearModifiers(user);
        ClientBridge.Instance.EquipmentVisuals.ClearEquipment(Definition.EquipmentType);
    }

    public override void Use(ItemDropInstance instance, IEntity user)
    {
        throw new InvalidOperationException($"[Item] {instance.BaseType.DisplayName} is an equipment item and cannot be used directly.");
    }
}