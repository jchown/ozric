using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using OzricUI.Components;

namespace OzricUI.Model;

[EditDialog(typeof(ConstantNumberDialog), "Number")]
public class ConstantNumberModel: ConstantModel
{
    public ConstantNumberModel(Constant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:alpha-x-circle-outline";
}