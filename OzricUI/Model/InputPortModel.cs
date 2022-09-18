using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class InputPortModel: GraphPortModel, IValueInput
{
    public InputPortModel(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is OutputPortModel output && valueType == output.valueType;
    }
}