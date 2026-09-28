using System;
using System.Collections.Generic;
using Godot;

public enum TracedShape
{
    Triangle,
    Circle,
    Square
}

[GlobalClass]
public partial class ShapeTraceComponent : CardComponent
{
    [ExportGroup("Detection tuning")]
    [Export] private float _cornerAngleThreshold = 35f;   // degrees; direction change needed to count as a corner
    [Export] private int _smoothingWindow = 3;            // compare point[i-w] to point[i+w] instead of neighbours, to reduce jitter
    [Export] private float _cornerMergeDistance = 0.75f;  // corners closer than this (world units) get collapsed into one
    [Export] private float _closureTolerancePct = 0.35f;  // end point must be within this % of shape size from start point

    private readonly List<Vector3> _tracePoints = new();
    private bool _isTracing;

    private TracedShape _expectedShape;
    private float _minSize;

    public event Action OnTraceStarted;
    public event Action<bool> OnTraceEvaluated; // true = matched expected shape

    public void Configure(TracedShape expectedShape, float minSize)
    {
        _expectedShape = expectedShape;
        _minSize = minSize;
    }

    protected override void OnActivate()
    {
        base.OnActivate();

        _tracePoints.Clear();
        _isTracing = true;
        OnTraceStarted?.Invoke();
    }

    public void EndTrace()
    {
        _isTracing = false;
        bool matched = Matches(_tracePoints, _expectedShape, _minSize);
        OnTraceEvaluated?.Invoke(matched);
    }

    public override void _Process(double delta)
    {
        if (!_isTracing) return;
        _tracePoints.Add(GlobalPosition);
    }

    private bool Matches(List<Vector3> points, TracedShape expected, float minSize)
    {
        if (points.Count < 4) return false; // not enough data to be any real shape

        List<Vector2> path = ToXZ(points);

        float size = BoundingSize(path);
        if (size < minSize)
            return false;

        if (!IsClosed(path, size))
            return false;

        int cornerCount = CountCorners(path);

        return expected switch
        {
            TracedShape.Circle => cornerCount <= 1,
            TracedShape.Triangle => cornerCount == 3,
            TracedShape.Square => cornerCount == 4,
            _ => false
        };
    }

    private static List<Vector2> ToXZ(List<Vector3> points)
    {
        List<Vector2> path = new(points.Count);
        foreach (var p in points)
            path.Add(new Vector2(p.X, p.Z));
        return path;
    }

    private static float BoundingSize(List<Vector2> path)
    {
        Vector2 min = path[0], max = path[0];
        foreach (var p in path)
        {
            min = min.Min(p);
            max = max.Max(p);
        }
        return (max - min).Length(); // bounding-box diagonal
    }

    private bool IsClosed(List<Vector2> path, float size)
    {
        float closeDist = path[0].DistanceTo(path[^1]);
        return closeDist <= size * _closureTolerancePct;
    }

    private int CountCorners(List<Vector2> path)
    {
        int w = Mathf.Max(1, _smoothingWindow);
        List<Vector2> rawCorners = new();

        for (int i = w; i < path.Count - w; i++)
        {
            Vector2 dirIn = (path[i] - path[i - w]).Normalized();
            Vector2 dirOut = (path[i + w] - path[i]).Normalized();
            float angle = Mathf.RadToDeg(dirIn.AngleTo(dirOut));
            angle = Mathf.Abs(angle);

            if (angle > _cornerAngleThreshold)
                rawCorners.Add(path[i]);
        }

        return MergeNearbyCorners(rawCorners, _cornerMergeDistance);
    }

    private static int MergeNearbyCorners(List<Vector2> rawCorners, float mergeDistance)
    {
        if (rawCorners.Count == 0) return 0;

        List<Vector2> merged = new() { rawCorners[0] };

        foreach (var c in rawCorners)
        {
            if (c.DistanceTo(merged[^1]) > mergeDistance)
                merged.Add(c);
        }

        // first/last merged corner might actually be the same corner if the shape wraps around
        if (merged.Count > 1 && merged[0].DistanceTo(merged[^1]) <= mergeDistance)
            merged.RemoveAt(merged.Count - 1);

        return merged.Count;
    }
}