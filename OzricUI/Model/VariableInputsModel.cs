using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public abstract class VariableInputsModel: GraphNodeModel
{
    public PortInputAdd plus;

    public VariableInputsModel(VariableInputs node, Point? point = null) : base(node, point)
    {
        plus = new PortInputAdd(this, node.valueType, PortAlignment.BottomLeft);
        AddPort(plus);
    }
    
    public void AddInput(Pin pin)
    {
        // Make sure the "add" port is always last
        
        RemovePort(plus);
        base.AddInput(pin);
        AddPort(plus);
    }
}