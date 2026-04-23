using Blazor.Diagrams.Core.Geometry;
using Ozric.Engine.Graph.Logic;
using ValueType = Ozric.Engine.Values.ValueType;

namespace Ozric.Dashboard.Model;

public class BinaryChoice: DiagramNode
{
    public BinaryChoice(Engine.Graph.Logic.BinaryChoice choice, Point? point = null): base(choice, point)
    {
    }
    
    public override string Icon => ICON;
    public const string ICON = "mdi:call-split";

    public static Engine.Graph.Logic.BinaryChoice Color(string nodeID)
    {
        return new Engine.Graph.Logic.BinaryChoice(nodeID, ValueType.Color);
    }
}