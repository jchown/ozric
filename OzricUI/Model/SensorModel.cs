using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class SensorModel: GraphNodeModel
{
    public const string ICON = "mdi:motion-sensor";

    public SensorModel(BinarySensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}