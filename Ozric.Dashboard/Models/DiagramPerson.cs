using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramPerson: DiagramNode
{
    public const string ICON = "mdi:person";

    public DiagramPerson(OzricEngine.Nodes.GraphPerson sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}