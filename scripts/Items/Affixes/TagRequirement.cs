using Godot;

[GlobalClass]
public partial class TagRequirement : Resource
{
    [Export] public bool MatchAll { get; set; }
    [Export] public TagContainer Tags { get; set; }

    public TagRequirement() { }

    public TagRequirement(TagContainer tags, bool matchAll = false)
    {
        Tags = tags;
        MatchAll = matchAll;
    }

    public static TagRequirement Empty => new(TagContainer.Empty, false);

    public bool IsEmpty => Tags == null || Tags.IsEmpty;

    public bool IsSatisfiedBy(TagContainer context)
    {
        if (IsEmpty) return true;
        if (context == null) return false;

        return MatchAll
            ? context.HasAll(Tags)
            : context.HasAny(Tags);
    }

    public bool IsElemental =>
        !MatchAll &&
        Tags != null &&
        Tags.HasTag(GameTags.RestrictionElementFire) &&
        Tags.HasTag(GameTags.RestrictionElementCold) &&
        Tags.HasTag(GameTags.RestrictionElementLightning);

    public override string ToString()
    {
        if (IsEmpty || Tags == null)
            return string.Empty;

        return Tags.ToString();
    }
}