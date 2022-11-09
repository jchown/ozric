using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class TweenModel: GraphNodeModel
{
    public TweenModel(Tween tween, Point? point = null): base(tween, point)
    {
    }
        
    public override string Icon => ICON;

    public const string ICON = "mdi:gradient";
}