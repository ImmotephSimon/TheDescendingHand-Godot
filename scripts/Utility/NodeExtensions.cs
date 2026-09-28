using System.IO;
using System.Runtime.CompilerServices;

public static class NodeExtensions
{
    // ---- Error reporting: one place for format + caller info ----

    private static void ReportMissing<T>(string where, string file, int line, string member) =>
        GD.PushError($"[Missing Component] {typeof(T).Name} {where} " +
                     $"(called from {member} in {Path.GetFileName(file)}:{line})");

    public static void PrintTree(this Node node, string indent = "")
    {
        GD.Print($"{indent}{node.Name} ({node.GetType().Name}) Owner={(node.Owner?.Name ?? "null")}");
        foreach (var child in node.GetChildren())
            child.PrintTree(indent + "  ");
    }



    // ---- Parent: for each ancestor (nearest first), check it and its direct children ----

    public static T GetComponentInParent<T>(this Node node, bool required = true, bool searchChildren = false,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        where T : class
    {
        Node cameFrom = null;
        for (var current = node; current != null; current = current.GetParent())
        {
            if (current is T self) return self;

            foreach (var child in current.GetChildren())
            {
                if (child == cameFrom) continue; // already covered on the way up

                if (searchChildren)
                {
                    var found = child.GetComponentInChildren<T>(required: false);
                    if (found != null) return found;
                }
                else if (child is T match)
                {
                    return match;
                }
            }

            cameFrom = current;
        }

        if (required) ReportMissing<T>($"in hierarchy above '{node?.Name}'", file, line, member);
        return null;
    }

    public static T GetComponentInOwner<T>(this Node node, bool required = true,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        where T : class
    {
        if (node == null)
        {
            ReportMissing<T>($" due to null Node.", file, line, member);
            return null;
        }

        var root = node.Owner;
        if (root == null)
        {
            ReportMissing<T>($" due to null Owner.", file, line, member);
            return null;
        }

        var result = root.GetComponentInChildren<T>(required: false);

        if (result == null && required)
            ReportMissing<T>($"in owner scene of '{node.Name}' (Owner: {root.Name})", file, line, member);
        return result;
    }

    // ---- Children: full recursive search, depth-first, includes runtime-added nodes ----

    public static T GetComponentInChildren<T>(this Node node, bool required = true,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0, [CallerMemberName] string member = "")
        where T : class
    {
        var result = node.GetComponentsInChildren<T>().FirstOrDefault();

        if (result == null && required)
            ReportMissing<T>($"in children below '{node.Name}' at {node.GetPath()}", file, line, member);
        return result;
    }

    public static IEnumerable<T> GetComponentsInChildren<T>(this Node node, bool includeSelf = true) where T : class
    {
        if (includeSelf && node is T self) yield return self;

        foreach (var child in node.GetChildren())
            foreach (var found in child.GetComponentsInChildren<T>(includeSelf: true))
                yield return found;
    }

    // ---- Try* wrappers ----

    public static bool TryGetComponent<T>(this Node node, out T result) where T : class
    {
        result = node.GetComponentInOwner<T>(required: false);
        return result != null;
    }

    public static bool TryGetComponentInParent<T>(this Node node, out T result) where T : class
    {
        result = node.GetComponentInParent<T>(required: false);
        return result != null;
    }

    public static bool TryGetComponentInChildren<T>(this Node node, out T result) where T : class
    {
        result = node.GetComponentInChildren<T>(required: false);
        return result != null;
    }
}