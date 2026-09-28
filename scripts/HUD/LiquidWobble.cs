
public enum LiquidGlobeType
{
    Life,
    Mana
}

[GlobalClass]
public partial class LiquidWobble : MeshInstance3D
{
    [Export] private float maxWobble = 0.03f;
    [Export] private float wobbleSpeed = 1.0f;
    [Export] private float recovery = 1.0f;
    [Export] private LiquidGlobeType type;

    [Export(PropertyHint.Range, "0,1,0.01")] public float fillPercentage = 0.5f;
    [Export] public Color lightColor = Colors.Red;
    [Export] public Color darkColor = new Color(0.2f, 0f, 0f);

    private ShaderMaterial shaderMaterial;
    private float targetFillPercentage;
    private float fillTimer;
    private const float FillDuration = 0.2f;
    private Vector3 lastPos;
    private Vector3 velocity;
    private Vector3 wobbleAmount;
    private Vector3 wobbleAmountToAdd;
    private float pulse;
    private float time;

    public override void _Ready()
    {

        var meshInstance = this.GetComponentInChildren<MeshInstance3D>();
        shaderMaterial = meshInstance.GetActiveMaterial(0) as ShaderMaterial;

        ConfigureTypeColors();
        lastPos = GlobalPosition;
        targetFillPercentage = fillPercentage;
    }

    private void ConfigureTypeColors()
    {
        switch (type)
        {
            case LiquidGlobeType.Life:
                lightColor = Colors.Red;
                darkColor = new Color(0.2f, 0f, 0f);
                break;
            case LiquidGlobeType.Mana:
                lightColor = Colors.Blue;
                darkColor = new Color(0f, 0f, 0.2f);
                break;
        }
    }

    public override void _Process(double delta)
    {
        float fDelta = (float)delta;
        time += fDelta;

        velocity = (GlobalPosition - lastPos) / fDelta;

        wobbleAmountToAdd.X += Mathf.Clamp((velocity.X + (velocity.Z * 0.2f)) * maxWobble, -maxWobble, maxWobble);
        wobbleAmountToAdd.Z += Mathf.Clamp((velocity.Z + (velocity.X * 0.2f)) * maxWobble, -maxWobble, maxWobble);

        wobbleAmount.X = Mathf.Lerp(wobbleAmount.X, 0, fDelta * recovery);
        wobbleAmount.Z = Mathf.Lerp(wobbleAmount.Z, 0, fDelta * recovery);

        pulse = 2 * Mathf.Pi * wobbleSpeed;
        wobbleAmountToAdd.X = Mathf.Lerp(wobbleAmountToAdd.X, 0, fDelta * recovery);
        wobbleAmountToAdd.Z = Mathf.Lerp(wobbleAmountToAdd.Z, 0, fDelta * recovery);

        float fillMultiplier = Mathf.Lerp(1.5f, 0.5f, fillPercentage);

        Vector3 currentTilt = new Vector3(
            (wobbleAmount.X + Mathf.Sin(pulse * time) * wobbleAmountToAdd.X) * fillMultiplier,
            0,
            (wobbleAmount.Z + Mathf.Cos(pulse * time) * wobbleAmountToAdd.Z) * fillMultiplier
        );
        lastPos = GlobalPosition;

        if (fillTimer < FillDuration)
        {
            fillTimer += fDelta;
            fillPercentage = Mathf.Lerp(fillPercentage, targetFillPercentage, fDelta / (FillDuration - fillTimer + fDelta));
        }

        UpdateShaderParameters(currentTilt);
    }

    private void UpdateShaderParameters(Vector3 currentTilt)
    {
        if (shaderMaterial == null) return;

        shaderMaterial.SetShaderParameter("current_tilt", currentTilt);
        shaderMaterial.SetShaderParameter("fill_percentage", fillPercentage);
        shaderMaterial.SetShaderParameter("light_color", lightColor);
        shaderMaterial.SetShaderParameter("dark_color", darkColor);
    }

    public void Initialize(PlayerStatsSync state)
    {
        switch (type)
        {
            case LiquidGlobeType.Life:
                state.HealthChanged += UpdateHealth;
                break;
            case LiquidGlobeType.Mana:
                state.ManaChanged += UpdateMana;
                break;
        }
    }

    private void UpdateMana(float current, float max, bool isInstant) => SetFill(current / max, isInstant);
    private void UpdateHealth(float current, float max, bool isInstant) => SetFill(current / max, isInstant);

    private void SetFill(float value, bool isInstant)
    {
        targetFillPercentage = value;
        if (isInstant)
        {
            fillPercentage = value;
            fillTimer = FillDuration;
        }
        else
        {
            fillTimer = 0f;
        }
    }
}