using Blazor.Diagrams.Core.Geometry;
using JetBrains.Annotations;
using OzricEngine.logic;

namespace OzricUI.Model;

public abstract class ConstantModel: GraphNodeModel
{
    public ConstantModel(Constant constant, Point? point = null): base(constant, point)
    {
    }
}