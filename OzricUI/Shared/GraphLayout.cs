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

    public string NewZoneID()
    {
        int i = zones.Count;
        string zoneID;
        do
        {
            zoneID = $"zone-{i++}";
        } while (zones.ContainsKey(zoneID));

        return zoneID;
    }

    public Zone AddZone(string zoneID)
    {
        var zone = new Zone(zoneID);
        zones[zoneID] = zone;
        return zone;
    }

    public void RemoveZone(string zoneID)
    {
        var zone = zones[zoneID];
        if (zone.nodeIDs.Count > 0)
            throw new Exception("ZOne contains nodes");

        zones.Remove(zoneID);
    }
}