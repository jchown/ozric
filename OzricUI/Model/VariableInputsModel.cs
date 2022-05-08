using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public abstract class VariableInputsModel: GraphNodeModel
{
    public VariableInputsModel(VariableInputs node, Point? point = null) : base(node, point)
    {
        foreach (var input in node.inputs)
        {
            AddInput(input.type, input.name);
        }
    }
}