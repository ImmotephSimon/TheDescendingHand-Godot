#if TOOLS
using Godot;
using System;

[Tool]
public partial class EditorToolsPlugin : EditorPlugin
{
    private static readonly (string Name, Action Action)[] Tools =
    {
        ("Import Passive Tree", PassiveTreeImporter.Import),
        // ("Some Other Tool", SomeOtherTool.Run),
    };

    public override void _EnterTree()
    {
        foreach (var (name, action) in Tools)
            AddToolMenuItem(name, Callable.From(action));
    }

    public override void _ExitTree()
    {
        foreach (var (name, _) in Tools)
            RemoveToolMenuItem(name);
    }
}
#endif

[AttributeUsage(AttributeTargets.Method)]
public class EditorToolAttribute : Attribute
{
    public string Name { get; }
    public EditorToolAttribute(string name) => Name = name;
}