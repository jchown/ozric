using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramModeSensor: DiagramNode
{
    public const string ICON = "mdi:label";

    public DiagramModeSensor(OzricEngine.Nodes.GraphModeSensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}