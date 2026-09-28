#if TOOLS
using Godot;
using System;
using System.Linq;
using System.Reflection;

[Tool]
public partial class ResourceGenerator : EditorScript
{
    private const string OutputDir = "res://Resources/Generated/";

    public override void _Run()
    {
        if (!DirAccess.DirExistsAbsolute(OutputDir))
        {
            var mkErr = DirAccess.MakeDirRecursiveAbsolute(OutputDir);
            if (mkErr != Error.Ok)
            {
                GD.PrintErr($"Could not create output dir: {OutputDir} err={mkErr}");
                return;
            }
        }

        // Find every non-abstract class in this assembly that derives from Resource
        // and has a public parameterless constructor.
        var resourceTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(Resource).IsAssignableFrom(t)
                        && !t.IsAbstract
                        && t.GetConstructor(Type.EmptyTypes) != null
                        && t != typeof(Resource))
            .ToList();

        GD.Print($"Found {resourceTypes.Count} Resource-derived types.");

        int created = 0;
        int failed = 0;

        foreach (var type in resourceTypes)
        {
            try
            {
                var instance = (Resource)Activator.CreateInstance(type);
                string savePath = $"{OutputDir}{type.Name}.tres";
                var err = ResourceSaver.Save(instance, savePath);

                if (err == Error.Ok)
                {
                    GD.Print($"Created: {savePath}");
                    created++;
                }
                else
                {
                    GD.PrintErr($"Failed to save {savePath}: {err}");
                    failed++;
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to instantiate {type.Name}: {e.Message}");
                failed++;
            }
        }

        GD.Print("---- Summary ----");
        GD.Print($"Created: {created}");
        GD.Print($"Failed: {failed}");

        EditorInterface.Singleton.GetResourceFilesystem().Scan();
    }
}
#endif