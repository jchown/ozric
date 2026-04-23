using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Entities;

namespace Ozric.Dashboard.Model;

public class DiagramSensor: DiagramNode, IAreaSource
{
    public const string ICON = "mdi:motion-sensor";

    public DiagramSensor(GraphBinarySensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}