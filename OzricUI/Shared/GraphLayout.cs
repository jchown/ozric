using OzricEngine;

namespace OzricUI;

public class GraphLayout: IEquatable<GraphLayout>
{
    public Dictionary<string, LayoutPoint> nodeLayout { get; set; } = new();

    #region Comparison
    public bool Equals(GraphLayout other)
    {
        return nodeLayout.SequenceEqual(other.nodeLayout);
    }
    
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((GraphLayout)obj);
    }

    public override int GetHashCode()
    {
        return nodeLayout.GetHashCode();
    }
    #endregion

    public override string ToString()
    {
        return Json.Serialize(this);
    }
}