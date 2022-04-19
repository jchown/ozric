using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class VariableInputsModel: GraphNodeModel
{
    public VariableInputsModel(VariableInputs node, Point? point = null) : base(node, point)
    {
        foreach (var input in node.inputs)
        {
            AddInput(input.type, input.name);
        }
    }
}