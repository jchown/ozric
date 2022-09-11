using Blazor.Diagrams.Core.Geometry;
using OzricEngine.logic;

namespace OzricUI.Model;

public class ColorConstantModel: ConstantModel
{
    public ColorConstantModel(Constant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:palette";
}