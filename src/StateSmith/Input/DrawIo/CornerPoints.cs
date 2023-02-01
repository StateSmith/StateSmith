#nullable enable


namespace StateSmith.Input.DrawIo;

public class CornerPoints
{
    /// <summary>
    /// coordinates are absolute within diagram root or collapsed node
    /// </summary>
    public double left, top, right, bottom;

    public bool ContainsPoint(double x, double y)
    {
        bool contains = x >= left && x <= right && y >= top && y <= bottom;
        return contains;
    }

    public bool ContainsCorner(CornerPoints other)
    {
        bool contains = false
            || ContainsPoint(x: other.left, y: other.top)
            || ContainsPoint(x: other.left, y: other.bottom)
            || ContainsPoint(x: other.right, y: other.top)
            || ContainsPoint(x: other.right, y: other.bottom);

        return contains;
    }

    public bool Overlaps(CornerPoints other)
    {
        return this.ContainsCorner(other) || other.ContainsCorner(this);
    }
}
