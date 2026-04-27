using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ConstantNumberDialog), "Number")]
public class DiagramConstantNumber: DiagramConstant
{
    public DiagramConstantNumber(Ozric.Engine.Nodes.GraphConstant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:alpha-x-circle-outline";
}