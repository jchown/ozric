using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public abstract class ConstantModel: GraphNodeModel
{
    public ConstantModel(Constant constant, Point? point = null): base(constant, point)
    {
    }
}