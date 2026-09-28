#if TOOLS
using System.Text.Json;

[Tool]
public partial class PassiveTreeImporter : EditorScript
{
    private const string DataPath = "res://data/PassiveTree";
    private const string NodesPath = DataPath + "/PassiveTreeNodes.json";
    private const string TypesPath = DataPath + "/NodeTypes.json";
    private const string NeighborsPath = DataPath + "/NodeNeighbors.json";
    private const string OutputPath = DataPath + "/PassiveTreeData.tres";
    private const string IconsPath = "res://textures/icons"; 

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    public static void Import()
    {
        PassiveNodeJson[] nodes = ReadJson<PassiveNodeJson[]>(NodesPath);
        PassiveNodeTypeJson[] types = ReadJson<PassiveNodeTypeJson[]>(TypesPath);
        PassiveNodeNeighborsJson[] neighbors = ReadJson<PassiveNodeNeighborsJson[]>(NeighborsPath);

        PassiveTreeData tree = LoadOrCreateTree();

        tree.Nodes.Clear();
        tree.NodeTypes.Clear();
        tree.Connections.Clear();

        ImportNodes(tree, nodes);
        ImportNodeTypes(tree, types);
        ImportConnections(tree, neighbors);

        Error err = ResourceSaver.Save(tree, OutputPath);
        if (err != Error.Ok)
        {
            GD.PrintErr($"Failed to save passive tree: {err}");
            return;
        }

        EditorInterface.Singleton.GetResourceFilesystem().Scan();

        GD.Print(
            $"Imported passive tree: {tree.Nodes.Count} nodes, " +
            $"{tree.NodeTypes.Count} types, " +
            $"{tree.Connections.Count} connections.");
    }

    private static T ReadJson<T>(string path)
    {
        using FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
            throw new System.IO.FileNotFoundException($"Could not open {path}: {FileAccess.GetOpenError()}");

        return JsonSerializer.Deserialize<T>(file.GetAsText(), JsonOptions);
    }

    private static PassiveTreeData LoadOrCreateTree()
    {
        if (!DirAccess.DirExistsAbsolute(DataPath))
            DirAccess.MakeDirRecursiveAbsolute(DataPath);

        if (ResourceLoader.Exists(OutputPath))
        {
            var existing = ResourceLoader.Load<PassiveTreeData>(OutputPath);
            if (existing != null)
                return existing;
        }

        return new PassiveTreeData();
    }

    private static void ImportNodes(PassiveTreeData tree, PassiveNodeJson[] source)
    {
        if (source == null || source.Length == 0)
            return;

        Vector2 min = new(float.MaxValue, float.MaxValue);
        Vector2 max = new(float.MinValue, float.MinValue);

        foreach (PassiveNodeJson node in source)
        {
            Vector2 position = new(node.Position.X, node.Position.Y);
            min = new Vector2(Mathf.Min(min.X, position.X), Mathf.Min(min.Y, position.Y));
            max = new Vector2(Mathf.Max(max.X, position.X), Mathf.Max(max.Y, position.Y));
        }

        Vector2 center = (min + max) * 0.5f;

        foreach (PassiveNodeJson node in source)
        {
            Vector2 position = new(node.Position.X, node.Position.Y);

            PassiveNode destination = new()
            {
                Name = node.Name,
                Type = node.Type,
                Position = position - center,
                Role = node.Role,
                Cluster = node.Cluster
            };

            if (node.Cost != null)
            {
                foreach (PassiveCostJson cost in node.Cost)
                {
                    destination.Cost.Add(new PassiveNodeCost
                    {
                        Amount = cost.Amount,
                        Attribute = new GameTag(CleanTag(cost.Attribute?.TagName))
                    });
                }
            }

            if (node.Requirement != null)
            {
                foreach (PassiveCostJson requirement in node.Requirement)
                {
                    destination.Requirement.Add(new PassiveNodeCost
                    {
                        Amount = requirement.Amount,
                        Attribute = new GameTag(CleanTag(requirement.Attribute?.TagName))
                    });
                }
            }

            tree.Nodes.Add(destination);
        }
    }

    private static void ImportNodeTypes(PassiveTreeData tree, PassiveNodeTypeJson[] source)
    {
        if (source == null) return;

        foreach (PassiveNodeTypeJson type in source)
        {
            PassiveNodeType destination = new()
            {
                Name = type.Name,
                Texture = FindTexture(type.Texture)
            };

            if (type.Modifier != null)
            {
                foreach (PassiveModifierJson modifier in type.Modifier)
                {
                    if (modifier?.Modifier == null || string.IsNullOrWhiteSpace(modifier.Modifier.TagName))
                    {
                        throw new InvalidOperationException(
                            $"[Import Failed] Node type '{type.Name}' has a modifier with a null or empty TagName.");
                    }

                    string cleanTagStr = CleanTag(modifier.Modifier.TagName);

                    if (!Enum.TryParse(modifier.MathOp, true, out MathOp mathOp))
                    {
                        throw new ArgumentException(
                            $"[Import Failed] Invalid MathOp '{modifier.MathOp}' on node type '{type.Name}'.");
                    }

                    destination.Modifiers.Add(new StatModifier(
                        new GameTag(cleanTagStr),
                        mathOp,
                        modifier.FinalValue));
                }
            }

            tree.NodeTypes.Add(destination);
        }
    }

    private static void ImportConnections(PassiveTreeData tree, PassiveNodeNeighborsJson[] source)
    {
        if (source == null) return;

        HashSet<string> connections = new();

        foreach (PassiveNodeNeighborsJson node in source)
        {
            if (node.Neighbors == null) continue;

            foreach (string neighbor in node.Neighbors)
            {
                string key = string.CompareOrdinal(node.Name, neighbor) < 0
                    ? $"{node.Name}|{neighbor}"
                    : $"{neighbor}|{node.Name}";

                if (!connections.Add(key))
                    continue;

                tree.Connections.Add(new PassiveNodeConnection
                {
                    A = node.Name,
                    B = neighbor
                });
            }
        }
    }

    private static string CleanTag(string rawTag)
    {
        if (string.IsNullOrWhiteSpace(rawTag))
            return string.Empty;

        int lastDot = rawTag.LastIndexOf('.');
        return (lastDot >= 0 && rawTag.EndsWith("'"))
            ? rawTag.Substring(lastDot + 1).TrimEnd('\'')
            : rawTag;
    }

    private static Texture2D FindTexture(string uePath)
    {
        if (string.IsNullOrWhiteSpace(uePath))
            return null;

        string fileName = System.IO.Path.GetFileNameWithoutExtension(uePath);
        List<string> matches = new();
        FindFiles(IconsPath, fileName, matches);

        if (matches.Count == 0)
        {
            GD.PrintErr($"Could not find texture for UE texture '{uePath}'.");
            return null;
        }

        if (matches.Count > 1)
            GD.PushWarning($"Multiple textures found for '{fileName}', using first match.");

        return ResourceLoader.Load<Texture2D>(matches[0]);
    }

    private static void FindFiles(string directory, string fileName, List<string> results)
    {
        using DirAccess dir = DirAccess.Open(directory);
        if (dir == null) return;

        dir.ListDirBegin();
        for (string entry = dir.GetNext(); entry != ""; entry = dir.GetNext())
        {
            if (entry.StartsWith(".")) continue;

            string fullPath = $"{directory}/{entry}";

            if (dir.CurrentIsDir())
            {
                FindFiles(fullPath, fileName, results);
            }
            else if (System.IO.Path.GetFileNameWithoutExtension(entry) == fileName
                     && !entry.EndsWith(".import"))
            {
                results.Add(fullPath);
            }
        }
        dir.ListDirEnd();
    }

    private class PassiveNodeJson
    {
        public string Name;
        public string Type;
        public PositionJson Position;
        public PassiveNodeRole Role;
        public PassiveCostJson[] Cost;
        public string Cluster;
        public PassiveCostJson[] Requirement;
    }

    private class PositionJson
    {
        public float X;
        public float Y;
    }

    private class PassiveCostJson
    {
        public int Amount;
        public AttributeJson Attribute;
    }

    private class AttributeJson
    {
        public string TagName;
    }

    private class PassiveNodeTypeJson
    {
        public string Name;
        public PassiveModifierJson[] Modifier;
        public string Texture;
    }

    private class PassiveModifierJson
    {
        public float FinalValue;
        public string MathOp;
        public string Restriction;
        public AttributeJson Modifier;
    }

    private class PassiveNodeNeighborsJson
    {
        public string Name;
        public string[] Neighbors;
    }
}
#endif