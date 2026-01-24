using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Logic;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ModeMatchDialog), "Mode Match")]
public class ModeMatchModel: GraphNodeModel
{
    public ModeMatchModel(ModeMatch modeMatch, Point? point = null): base(modeMatch, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "icon-park-solid:checklist";
}