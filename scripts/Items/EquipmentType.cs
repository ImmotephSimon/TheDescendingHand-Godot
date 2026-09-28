[GlobalClass]
public partial class EquipmentType : Resource
{
    private static readonly Dictionary<Guid, EquipmentType> Registry = new();

    public Guid Id => GuidUtils.Deterministic(ResourcePath);

    [Export] public ModifierPool ModifierPool { get; set; }

    public static bool TryGet(Guid id, out EquipmentType type) => Registry.TryGetValue(id, out type);

    public void Register() => Registry[Id] = this;

}