[GlobalClass]
public partial class TagReactions : Resource
{
    [Export] public ReactionType[] Reactions { get; set; }
}