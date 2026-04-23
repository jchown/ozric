using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public class ModeSensor: DiagramNode
{
    public const string ICON = "mdi:label";

    public ModeSensor(OzricEngine.Nodes.ModeSensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}