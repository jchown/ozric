using Blazor.Diagrams.Core.Geometry;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ConstantColorDialog), "Color")]
public class DiagramConstantColor: DiagramConstant
{
    public DiagramConstantColor(OzricEngine.Nodes.GraphConstant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:palette";
}