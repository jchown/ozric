using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramIfAll: DiagramVariableInputs
{
    public DiagramIfAll(Engine.Graph.Logic.GraphIfAll ifAll, Point? point = null): base(ifAll, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:gate-and";
}