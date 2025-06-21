using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Logic;
using OzricEngine.Nodes;
using OzricUI.Components;

namespace OzricUI.Model;

[EditDialog(typeof(NumberCompareDialog), "Number Compare")]
public class NumberCompareModel: GraphNodeModel
{
    public NumberCompareModel(NumberCompare NumberCompare, Point? point = null): base(NumberCompare, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "tabler:ruler-measure";
}