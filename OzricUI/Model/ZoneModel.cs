using Blazor.Diagrams.Core.Models;

public class ZoneModel : GroupModel
{
    public readonly string zoneID;

    public ZoneModel(string zoneID): base(new List<NodeModel>())
    {
        this.zoneID = zoneID;
    }
}