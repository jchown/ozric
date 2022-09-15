using Blazor.Diagrams.Core.Models;
using OzricEngine.logic;

namespace OzricUI.Model;

public class InputPortModel: GraphPortModel
{
    public InputPortModel(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is OutputPortModel output && valueType == output.valueType;
    }
}