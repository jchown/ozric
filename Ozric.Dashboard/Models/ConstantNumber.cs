using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ConstantNumberDialog), "Number")]
public class ConstantNumber: Constant
{
    public ConstantNumber(OzricEngine.Nodes.Constant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:alpha-x-circle-outline";
}