using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ModeMatchDialog), "Mode Match")]
public class DiagramModeMatch: DiagramNode
{
    public DiagramModeMatch(Engine.Graph.Logic.GraphModeMatch modeMatch, Point? point = null): base(modeMatch, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "icon-park-solid:checklist";
}