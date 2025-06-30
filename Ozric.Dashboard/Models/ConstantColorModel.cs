using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using Ozric.Dashboard.Components;

namespace Ozric.Dashboard.Model;

[EditDialog(typeof(ConstantColorDialog), "Color")]
public class ConstantColorModel: ConstantModel
{
    public ConstantColorModel(Constant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:palette";
}