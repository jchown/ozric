using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class SwitchModel: GraphNodeModel
{
    public const string ICON = "mdi:electric-switch";

    public SwitchModel(Switch @switch, Point? point = null): base(@switch, point)
    {
        AddPort(new InputBoolean(Switch.INPUT_NAME, this));
    }
    
    public override string Icon => ICON;
}