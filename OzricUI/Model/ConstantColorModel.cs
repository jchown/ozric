using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class ConstantColorModel: ConstantModel
{
    public ConstantColorModel(Constant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:palette";
}