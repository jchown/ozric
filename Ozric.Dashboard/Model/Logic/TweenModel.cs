using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Logic;
using OzricEngine.Nodes;

namespace Ozric.Dashboard.Model;

public class TweenModel: GraphNodeModel
{
    public TweenModel(Tween tween, Point? point = null): base(tween, point)
    {
    }
        
    public override string Icon => ICON;

    public const string ICON = "mdi:gradient";
}