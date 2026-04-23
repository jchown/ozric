using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;
using Ozric.Engine.Graph.Logic;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(NumberCompareDialog), "Number Compare")]
public class NumberCompare: DiagramNode
{
    public NumberCompare(Engine.Graph.Logic.NumberCompare NumberCompare, Point? point = null): base(NumberCompare, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "tabler:ruler-measure";
}