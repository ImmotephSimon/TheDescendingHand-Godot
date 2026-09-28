
[GlobalClass]
public partial class TagContainer : Resource
{
    [Export] public Godot.Collections.Array<GameTag> Tags { get; set; } = new();

    public static TagContainer Empty => new();

    public GameTag PrimaryTag => Tags != null && Tags.Count > 0 ? Tags[0] : null;

    public bool IsEmpty => Tags == null || Tags.Count == 0;

    public TagContainer() { }

    public TagContainer(GameTag tag)
    {
        if (tag != null)
            Tags.Add(tag);
    }

    public TagContainer(TagContainer other)
    {
        if (other?.Tags != null)
            Tags = new(other.Tags);
    }

    public TagContainer With(GameTag tag)
    {
        var copy = new TagContainer(this);
        if (tag != null)
            copy.Tags.Add(tag);
        return copy;
    }

    public bool HasTag(GameTag tag)
    {
        if (tag == null || string.IsNullOrEmpty(tag.TagId)) return false;
        if (Tags == null) return false;

        for (int i = 0; i < Tags.Count; i++)
        {
            var existingId = Tags[i]?.TagId;
            if (existingId == null) continue;

            if (existingId == tag.TagId)
                return true;

            if (existingId.StartsWith(tag.TagId, StringComparison.Ordinal) &&
                existingId.Length > tag.TagId.Length &&
                existingId[tag.TagId.Length] == '.')
            {
                return true;
            }
        }

        return false;
    }

    public bool HasAny(TagContainer required)
    {
        if (required == null || required.IsEmpty) return true;

        for (int i = 0; i < required.Tags.Count; i++)
        {
            if (HasTag(required.Tags[i]))
                return true;
        }

        return false;
    }

    public bool HasAll(TagContainer required)
    {
        if (required == null || required.IsEmpty) return true;

        for (int i = 0; i < required.Tags.Count; i++)
        {
            if (!HasTag(required.Tags[i]))
                return false;
        }

        return true;
    }

    public void AddRange(IEnumerable<GameTag> tags)
    {
        if (tags == null) return;

        foreach (var tag in tags)
            Add(tag);
    }

    public void Add(GameTag tag)
    {
        if (tag != null)
        {
            Tags ??= new();
            Tags.Add(tag);
        }
    }

    public override string ToString()
    {
        if (IsEmpty || PrimaryTag == null) return string.Empty;
        return PrimaryTag.ToString();
    }
}