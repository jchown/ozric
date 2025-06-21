using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard;

public record LayoutPoint(double x, double y)
{
    public Point ToPoint()
    {
        return new Point(x, y);
    }

    public static LayoutPoint FromPoint(Point point)
    {
        return new LayoutPoint(point.X, point.Y);
    }
}