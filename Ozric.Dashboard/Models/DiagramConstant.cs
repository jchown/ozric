using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public abstract class DiagramConstant: DiagramNode
{
    public DiagramConstant(GraphConstant constant, Point? point = null): base(constant, point)
    {
    }
}