using Blazor.Diagrams.Core.Geometry;

namespace OzricUI;

public class GraphLayout
{
    protected bool Equals(GraphLayout other)
    {
        return nodeLayout.Equals(other.nodeLayout);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((GraphLayout)obj);
    }

    public override int GetHashCode()
    {
        return nodeLayout.GetHashCode();
    }

    public Dictionary<string, Point> nodeLayout = new();
}