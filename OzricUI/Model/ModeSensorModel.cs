using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class ModeSensorModel: GraphNodeModel
{
    public const string ICON = "mdi:label";

    public ModeSensorModel(ModeSensor sensor, Point? point = null): base(sensor, point)
    {
    }
    
    public override string Icon => ICON;
}