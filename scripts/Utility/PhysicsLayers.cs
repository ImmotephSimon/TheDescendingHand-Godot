public static class PhysicsLayers
{
    public const uint Default = 1u << 0; // Layer 1
    public const uint Player = 1u << 1; // Layer 2
    public const uint Enemy = 1u << 2; // Layer 3
    public const uint PlayerAttack = 1u << 3; // Layer 4
    public const uint EnemyAttack = 1u << 4; // Layer 5

#if DEBUG
    public static void ValidateNames()
    {
        var fields = typeof(PhysicsLayers).GetFields(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        foreach (var field in fields)
        {
            uint value = (uint)field.GetValue(null);
            int layerNumber = (int)System.Math.Log2(value) + 1;

            string actual = ProjectSettings.GetSetting($"layer_names/3d_physics/layer_{layerNumber}").AsString();
            Debug.Assert(actual == field.Name,
                $"Layer {layerNumber} named '{actual}' in Project Settings, expected '{field.Name}'");
        }
    }
#endif
}