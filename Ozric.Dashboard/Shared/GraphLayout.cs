using Ozric.Engine.Extensions;
using OzricEngine;

namespace Ozric.Dashboard.Shared;

public class GraphLayout: IEquatable<GraphLayout>
{
    public int version { get; set; }

    public Dictionary<string, Dictionary<string, LayoutPoint>> nodeLayoutsPerArea { get; set; } = new();
    
    public LayoutPoint? GetNodePosition(string areaId, string nodeId)
    {
        if (!nodeLayoutsPerArea.TryGetValue(areaId, out var areaLayouts))
            return null;

        if (!areaLayouts.TryGetValue(nodeId, out var layoutPoint))
            return null;

        return layoutPoint;
    }

    public void SetNodePosition(string areaId, string nodeId, LayoutPoint point)
    {
        var areaLayouts = nodeLayoutsPerArea.GetOrSet(areaId, () => new());
        areaLayouts[nodeId] = point;
    }

    public void RemoveNode(string areaId, string nodeId)
    {
        if (nodeLayoutsPerArea.TryGetValue(areaId, out var areaLayouts))
        {
            areaLayouts.Remove(nodeId);
        }
    }

    public void RemapNodeID(string originalId, string newId)
    {
        foreach (var areaLayouts in nodeLayoutsPerArea.Values)
        {
            if (areaLayouts.TryGetValue(originalId, out var layoutPoint))
            {
                areaLayouts.Remove(originalId);
                areaLayouts[newId] = layoutPoint;
            }
        }
    }

    public void MigrateToNormalized(double containerWidth, double containerHeight)
    {
        foreach (var areaLayouts in nodeLayoutsPerArea.Values)
        {
            var keys = areaLayouts.Keys.ToList();
            foreach (var key in keys)
            {
                var point = areaLayouts[key];
                areaLayouts[key] = new LayoutPoint(point.x / containerWidth, point.y / containerHeight);
            }
        }

        version = 2;
    }

    #region Comparison
    public bool Equals(GraphLayout? other)
    {
        if (other == null)
            return false;
        
        return nodeLayoutsPerArea.SequenceEqual(other.nodeLayoutsPerArea);
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
        return nodeLayoutsPerArea.GetHashCode();
    }
    #endregion

    public override string ToString()
    {
        return Json.Serialize(this);
    }
}