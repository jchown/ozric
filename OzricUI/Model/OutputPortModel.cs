using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;

namespace OzricUI.Model;

internal class OutputPortModel: GraphPortModel
{
    public OutputPortModel(NodeModel parent, Pin pin, PortAlignment alignment) : base(parent, pin, alignment)
    {
    }
    
    public override bool CanAttachTo(PortModel port)
    {
        return port is InputPortModel input && valueType == input.valueType;
    }
}