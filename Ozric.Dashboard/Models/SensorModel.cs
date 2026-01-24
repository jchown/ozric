using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Entities;

namespace Ozric.Dashboard.Model;

public class SensorModel: GraphNodeModel, IAreaSource
{
    public const string ICON = "mdi:motion-sensor";

    public SensorModel(BinarySensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}