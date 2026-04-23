using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramIfAny: DiagramVariableInputs
{
    public DiagramIfAny(Engine.Graph.Logic.GraphIfAny ifAny, Point? point = null): base(ifAny, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:gate-or";
}