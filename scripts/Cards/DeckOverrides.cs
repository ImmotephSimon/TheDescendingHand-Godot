[GlobalClass]
public partial class DeckOverrides : Resource
{
    [Export] private int _addedDraws = 0;
    [Export] private int _addedDiscards = 0;

    public int AddedDraws => _addedDraws;
    public int AddedDiscards => _addedDiscards;

    public DeckOverrides() { }
}