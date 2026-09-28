[GlobalClass]
public partial class EquipComponentDefinition : ItemComponentDefinition
{
    [Export] private EquipmentType equipmentType;
    public EquipmentType EquipmentType => equipmentType;

    public override ItemUseComponent CreateRuntimeComponent()
    {
        return new EquipUseComponent(this);
    }
}