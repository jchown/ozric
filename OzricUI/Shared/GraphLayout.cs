using Blazor.Diagrams.Core.Models;
using OzricEngine;

namespace OzricUI;

public class GraphLayout: IEquatable<GraphLayout>
{
    public Dictionary<string, LayoutPoint> nodeLayout { get; set; } = new();
    public Dictionary<string, Zone> zones { get; set; } = new();

    #region Comparison
    public bool Equals(GraphLayout? other)
    {
        if (other == null)
            return false;
        
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

    public Zone AddZone(List<string> nodeIDs)
    {
        int i = zones.Count;
        string zoneID;
        do
        {
            zoneID = $"zone-{i++}";
        } while (zones.ContainsKey(zoneID));

        var zone = new Zone(zoneID, nodeIDs);
        zones[zoneID] = zone;
        return zone;
    }
}