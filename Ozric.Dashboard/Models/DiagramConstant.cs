using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public abstract class DiagramConstant: DiagramNode
{
    public DiagramConstant(OzricEngine.Nodes.GraphConstant constant, Point? point = null): base(constant, point)
    {
    }
}