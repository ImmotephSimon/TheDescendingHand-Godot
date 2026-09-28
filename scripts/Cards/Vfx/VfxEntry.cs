[GlobalClass]
public partial class VfxEntry : Resource
{
    [Export] private string id;
    [Export] private PackedScene prefab;

    public string Id => id;
    public PackedScene Prefab => prefab;

    public void SetId(string value) => id = value;
}