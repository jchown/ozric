using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class ModeSwitchModel: GraphNodeModel
{
    public ModeSwitchModel(ModeSwitch s, Point? point = null): base(s, point)
    {
        AddPort(new InputOnOff(ModeSwitch.INPUT_NAME, this));
        
        foreach (var output in s.outputs)
        {
            AddOutput(output.type, output.name);
        }

    }
}