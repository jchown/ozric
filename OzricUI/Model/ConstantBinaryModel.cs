using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using OzricEngine.Values;
using OzricUI.Components;

namespace OzricUI.Model;

[EditDialog(typeof(ConstantBinaryDialog), "Binary")]
public class ConstantBinaryModel: ConstantModel
{
    public ConstantBinaryModel(Constant constant, Point? point = null): base(constant, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:toggle-switch";
}