using Blazor.Diagrams.Core.Geometry;
using OzricEngine.Nodes;
using ValueType = OzricEngine.Values.ValueType;

namespace OzricUI.Model;

public class BinaryChoiceModel: GraphNodeModel
{
    public BinaryChoiceModel(BinaryChoice choice, Point? point = null): base(choice, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:call-split";

    public static BinaryChoice Color(string nodeID)
    {
        return new BinaryChoice(nodeID, ValueType.Color);
    }
}