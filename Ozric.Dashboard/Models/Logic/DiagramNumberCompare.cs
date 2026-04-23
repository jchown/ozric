using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(NumberCompareDialog), "Number Compare")]
public class DiagramNumberCompare: DiagramNode
{
    public DiagramNumberCompare(Engine.Graph.Logic.GraphNumberCompare DiagramNumberCompare, Point? point = null): base(DiagramNumberCompare, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "tabler:ruler-measure";
}