using Blazor.Diagrams.Core.Geometry;

namespace Ozric.Dashboard.Model;

public class DiagramTween: DiagramNode
{
    public DiagramTween(Engine.Graph.Logic.GraphTween tween, Point? point = null): base(tween, point)
    {
    }
        
    public override string Icon => ICON;

    public const string ICON = "mdi:gradient";
}