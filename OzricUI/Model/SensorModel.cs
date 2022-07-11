using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class SensorModel: GraphNodeModel
{
    public const string ICON = "mdi:motion-sensor";

    public SensorModel(Sensor sensor, Point? point = null): base(sensor, point)
    {
        AddPort(new OutputOnOff(Sensor.OUTPUT_NAME, this));
    }
    
    public override string Icon => ICON;
}