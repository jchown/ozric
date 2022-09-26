using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class BinaryChoiceModel: GraphNodeModel
{
    public BinaryChoiceModel(BinaryChoice choice, Point? point = null): base(choice, point)
    {
    }
    
    public override string Icon => ICON;

    public const string ICON = "mdi:call-split";
}