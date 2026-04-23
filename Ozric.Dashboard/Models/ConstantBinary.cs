using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ConstantBinaryDialog), "Binary")]
public class ConstantBinary: Constant
{
    public ConstantBinary(OzricEngine.Nodes.Constant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:toggle-switch";
}