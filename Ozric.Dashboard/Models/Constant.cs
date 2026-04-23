using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public abstract class Constant: DiagramNode
{
    public Constant(OzricEngine.Nodes.Constant constant, Point? point = null): base(constant, point)
    {
    }
}