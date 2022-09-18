using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public abstract class VariableInputsModel: GraphNodeModel
{
    public VariableInputPortModel plus;

    public VariableInputsModel(VariableInputs node, Point? point = null) : base(node, point)
    {
        plus = new VariableInputPortModel(this, node.valueType, PortAlignment.BottomLeft);
        AddPort(plus);
    }
}