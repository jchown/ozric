using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class SwitchModel: GraphNodeModel
{
    public SwitchModel(Switch s, Point? point = null): base(s, point)
    {
        AddPort(new InputOnOff(Switch.INPUT_NAME_SWITCH, this));
        AddInput(s.valueType, Switch.INPUT_NAME_ON);
        AddInput(s.valueType, Switch.INPUT_NAME_OFF);
        AddOutput(s.valueType, Switch.OUTPUT_NAME);
    }
    
    public override string Icon => "mdi:switch";
}