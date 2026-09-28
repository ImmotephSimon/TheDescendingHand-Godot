[GlobalClass]
public partial class BalanceCurves : Resource
{
    [Export] public Curve ExpectedPlayerLife { get; set; } = CreateDefaultCurve();

    private static Curve CreateDefaultCurve()
    {
        var curve = new Curve { MinDomain = 1f, MaxDomain = 100f, MinValue = 0f, MaxValue = 250f };
        curve.AddPoint(new Vector2(1f, 20f));
        curve.AddPoint(new Vector2(100f, 218f));
        var p0 = curve.GetPointPosition(0);
        var p1 = curve.GetPointPosition(1);
        float slope = (p1.Y - p0.Y) / (p1.X - p0.X);
        curve.SetPointLeftTangent(0, slope);
        curve.SetPointRightTangent(0, slope);
        curve.SetPointLeftTangent(1, slope);
        curve.SetPointRightTangent(1, slope);
        return curve;
    }
}