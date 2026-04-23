using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Logic;

namespace Ozric.Dashboard.Model;

public class Tween: DiagramNode
{
    public Tween(Engine.Graph.Logic.Tween tween, Point? point = null): base(tween, point)
    {
    }
        
    public override string Icon => ICON;

    public const string ICON = "mdi:gradient";
}