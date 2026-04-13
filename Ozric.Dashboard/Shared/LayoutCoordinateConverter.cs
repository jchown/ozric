using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Shared;

public static class LayoutCoordinateConverter
{
    public static Point ToPixels(LayoutPoint normalized, Rectangle container)
    {
        return new Point(normalized.x * container.Width, normalized.y * container.Height);
    }

    public static LayoutPoint ToNormalized(Point pixel, Rectangle container)
    {
        return new LayoutPoint(pixel.X / container.Width, pixel.Y / container.Height);
    }
}
