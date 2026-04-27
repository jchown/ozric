using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Nodes;

namespace Ozric.Dashboard.Model;

public abstract class DiagramConstant: DiagramNode
{
    public DiagramConstant(GraphConstant constant, Point? point = null): base(constant, point)
    {
    }
}