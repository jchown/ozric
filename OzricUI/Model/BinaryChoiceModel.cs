using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;

namespace OzricUI.Model;

public class BooleanChoiceModel: GraphNodeModel
{
    public BooleanChoiceModel(BinaryChoice s, Point? point = null): base(s, point)
    {
    }
    
    public override string Icon => "mdi:switch";
}