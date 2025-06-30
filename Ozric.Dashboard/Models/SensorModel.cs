using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public class SensorModel: GraphNodeModel, IAreaSource
{
    public const string ICON = "mdi:motion-sensor";

    public SensorModel(BinarySensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}