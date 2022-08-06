using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class SwitchModel: GraphNodeModel
{
    public SwitchModel(BooleanChoice s, Point? point = null): base(s, point)
    {
        AddPort(new InputOnOff(BooleanChoice.INPUT_NAME_SWITCH, this));
        AddInput(s.valueType, BooleanChoice.INPUT_NAME_ON);
        AddInput(s.valueType, BooleanChoice.INPUT_NAME_OFF);
        AddOutput(s.valueType, BooleanChoice.OUTPUT_NAME);
    }
    
    public override string Icon => "mdi:switch";
}