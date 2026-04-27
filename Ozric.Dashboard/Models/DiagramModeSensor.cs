using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Model;

namespace Ozric.Dashboard.Models;

public class DiagramModeSensor: DiagramNode
{
    public const string ICON = "mdi:label";

    public DiagramModeSensor(Ozric.Engine.Nodes.GraphModeSensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}